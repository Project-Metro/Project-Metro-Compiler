; sources
; http://3zanders.co.uk/2017/10/13/writing-a-bootloader/
; https://en.wikipedia.org/wiki/BIOS_interrupt_call
; http://www.ctyme.com/intr/rb-0069.htm
; enable a20: ax = 2401h, int 15h http://www.ctyme.com/intr/rb-1336.htm
; disable a20: ax = 2400h, int 15h http://www.ctyme.com/intr/rb-1335.htm
; https://wiki.osdev.org/GDT
; http://web.archive.org/web/20190424213806/http://www.osdever.net/tutorials/view/the-world-of-protected-mode
; http://www.brackeen.com/vga/basics.html

; stuff to research
; https://wiki.osdev.org/CPU_Registers_x86
; https://stackoverflow.com/questions/43839960/is-eflags-a-general-purpose-register



bits 16 ; instruction for nasm
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

    cli

    ; load the data segment register (ds) with the address for the gdt-descriptor - this is DS:gdt_desc (offset shorthand)
    ; we can't set the register directly - why?
    ;xor ax, ax ; zero out ax
    ;mov ds, ax ; move register ax (value: 0) into ds

    lgdt [GDT32.Pointer] ; load global descriptor table (gdt) with a pointer to the descriptor

    ; enabling protected mode
    mov eax, cr0
    or eax, 1
    mov cr0, eax

    ; long jump (apparently clears the instruction pipeline)
    jmp GDT32.Code:now_protected_boot


bits 32 ; nasm instruction

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

    mov esi, boot_msg
    mov ebx, 0xb8000 ; vga memory start
    call printer

    mov esi, mode_msg
    mov ebx, 0xb80A0
    call printer

    hlt

boot_msg    db "UoL UVROS - Project Metro", 0
mode_msg    db "Hello World in 32bit! (protected mode)", 0