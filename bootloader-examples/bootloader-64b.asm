org 0x7c00

entry:
    jmp real_to_protected

; import GDTs for other modes
%include "GDT32.asm"
%include "GDT64.asm"

; nasm directive
bits 16 

; 16 bits to 32 bits
real_to_protected:

    ; enable a20 gate
    mov ax, 0x2401
    int 0x15

    ; change video mode
    mov ax, 0x3
    int 0x10

    cli
    lgdt [GDT32.Pointer]

    ; enable protected mode
    mov eax, cr0
    or eax, 1
    mov cr0, eax

    ; perform long jump
    jmp GDT32.Code:protected_to_long


[bits 32]
protected_to_long:

    ; load registers with GDT data segment offset
    mov ax, GDT32.Data
    mov ds, ax
    mov fs, ax
    mov gs, ax
    mov ss, ax


    ; root table - page-map level-4 table (PM4T)
    mov edi, 0x1000 ; starting address of 0x1000
    mov cr3, edi    ; move base address of page entry into control register 3 (https://wiki.osdev.org/CPU_Registers_x86)

    xor eax, eax    ; set EAX to 0

    mov ecx, 4096
    rep stosd   ; for ECX times, store EAX value at whatever position EDI points to, incrementing/decrementing as you go
                ; https://stackoverflow.com/questions/3818856/what-does-the-rep-stos-x86-assembly-instruction-sequence-do
    
    mov edi, cr3 ; restore original starting address

    ; according to https://wiki.osdev.org/Setting_Up_Long_Mode , this will set up the pointers to the other tables
    ; using an offset of 0x0003 from the destination address supposedly sets the bits to indicate that the page is present
    ;   and is also readable/writeable
    mov dword [edi], 0x2003
    add edi, 0x1000
    mov dword [edi], 0x3003
    add edi, 0x1000
    mov dword [edi], 0x4003
    add edi, 0x1000

    ; at this stage:
    ; PML4T is at 0x1000
    ; PDPT is at 0x2000
    ; PDT is at 0x3000
    ; PT is at 0x4000

    ; used to identity map the first 2MiB (see https://wiki.osdev.org/Setting_Up_Long_Mode)
    mov ebx, 0x00000003
    mov ecx, 512

    .set_entry:
        mov dword [edi], ebx
        add ebx, 0x1000
        add edi, 8
        loop .set_entry

    ; enable pae-paging by setting the appropriate bit in the control register
    mov eax, cr4
    or eax, 1 << 5
    mov cr4, eax

    mov ecx, 0xC0000080     ; magic value actually refers to the EFER MSR 
                            ;       -> 'extended feature enable register : model specific register
    rdmsr                   ; read model specific register
    or eax, 1 << 8          ; set long-mode bit (bit 8)
    wrmsr                   ; write back to model specific register

    mov eax, cr0
    or eax, 1 << 31 | 1 << 0         ; set PG bit (31st) & PM bit (0th)
    mov cr0, eax

    ; ^ this has now entered us into 32b compatability submodee

    lgdt [GDT64.Pointer]
    jmp GDT64.Code:real_long_mode

[bits 64]

printer:
    printer_loop:
        lodsb
        or al, al
        jz printer_exit

        or rax, 0x0F00
        mov qword [rbx], rax
        add rbx, 2
        jmp printer_loop

    printer_exit:
        ret

real_long_mode:
    cli

    mov ax, GDT64.Data
    mov ds, ax
    mov fs, ax
    mov gs, ax
    mov ss, ax

    ; clears out register RAX - if commented out then weird orange square is drawn at the end of the string
    xor rax, rax 

    mov rsi, boot_msg
    mov rbx, 0xb8000
    call printer

    mov rsi, l_mode
    mov rbx, 0xb80A0
    call printer

    hlt

boot_msg db "UoL UVROS - Project Metro",0
l_mode db "Hello World in 64bit! (long mode)",0
times 510 - ($-$$) db 0
dw 0xaa55