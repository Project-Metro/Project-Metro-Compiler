; sources
; http://3zanders.co.uk/2017/10/13/writing-a-bootloader/
; https://wiki.osdev.org/BIOS
; https://www.tutorialspoint.com/assembly_programming/assembly_registers.htm

bits 16
org 0x7c00

entry:
    jmp boot

printer:
    mov ah, 0x0e    ; set specific video mode (write/display character): see https://wiki.osdev.org/BIOS

    printer_loop:
        lodsb               ; loads a byte at address DS:SI into AL
        or al, al           ; if the (string) byte at AL is 0
        jz printer_end      ; it means we've hit the end of our string and can return
        int 0x10            ; interrupt call to BIOS to write to screen
        jmp printer_loop    ; loop to finish string
    
    printer_end:
        ret

boot: 
    mov si, boot_msg    ; loads the "source index" register with the message 
    call printer        ; print message

    mov si, mode_msg
    call printer


boot_msg    db  10, "UoL UVROS - Project Metro", 13, 10, 0 ;carrige return, newline, null-terminate
mode_msg    db  "Hello World in 16bit! (real mode)", 13, 10, 0


; bootloader magic padding until 512 bytes with magic pair
times 510 - ($ - $$) db 0
dw 0xaa55