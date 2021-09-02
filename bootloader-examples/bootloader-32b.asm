; 32 BIT BOOTLOADER

; NASM directives
; https://www.nasm.us/xdoc/2.13.03/html/nasmdoc6.html
bits 16
org 0x7c00

entry:
    jmp boot

%include "GDT32.asm"

boot:
    ; enabling a20 gate
    mov ax, 0x2401
    int 0x15

    ; changing to text mode (http://www.brackeen.com/vga/basics.html)
    mov ax, 0x3
    int 0x10

    ; disable interrupts
    cli

    ; load global descriptor table (gdt) with a pointer to the descriptor
    lgdt [GDT32.Pointer] 

    ; enabling protected mode
    mov eax, cr0
    or eax, 1
    mov cr0, eax

    ; long jump
    jmp GDT32.Code:now_protected_boot

; NASM directive
bits 32

printer:
    printer_loop:
        lodsb
        or al, al
        jz printer_end
        or eax, 0x0F00
        mov word [ebx], ax
        add ebx, 2
        jmp printer_loop

    printer_end:
        ret


now_protected_boot:
    mov ax, GDT32.Data      ; --|
    mov ds, ax              ;   |
    mov ss, ax              ;   | - loading up the segment registers with the data segment position
    mov fs, ax              ;   |
    mov gs, ax              ; --|

    ; since we are in protected mode, we can no longer use bios interrupts to perform functions
    ; so we write directly to VGA memory (starting at 0xb8000)
    mov esi, boot_msg
    mov ebx, 0xb8000
    call printer

    mov esi, mode_msg
    mov ebx, 0xb80A0
    call printer

    hlt

boot_msg    db "UoL UVROS - Project Metro", 0
mode_msg    db "Hello World in 32bit! (protected mode)", 0

times 510 - ($-$$) db 0
dw 0xaa55