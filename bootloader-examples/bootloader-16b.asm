; 16 BIT BOOTLOADER

; NASM directives
; https://www.nasm.us/xdoc/2.13.03/html/nasmdoc6.html
bits 16
org 0x7c00

entry:
    jmp boot

printer:
    ; set specific video mode (write/display character): see https://wiki.osdev.org/BIOS
    mov ah, 0x0e    

    printer_loop:
        lodsb               ; loads a byte at address DS:SI into AL
        or al, al           ; if the (string) byte at AL is 0...
        jz printer_end      ; ...it means we've hit the end of our string and can return
        int 0x10            ; interrupt call to BIOS to write to screen
        jmp printer_loop    ; loop to finish string
    
    printer_end:
        ret

boot: 
    mov si, boot_msg    ; loads the "source index" register with the message 
    call printer        ; print message

    mov si, mode_msg
    call printer

    hlt ; stop the processor

; 13 = carrige return, 10 = newline, 0 = null-terminate
boot_msg    db  10, "UoL UVROS - Project Metro", 13, 10, 0
mode_msg    db  "Hello World in 16bit! (real mode)", 13, 10, 0


; bootloader magic padding until 512 bytes with magic key
times 510 - ($ - $$) db 0
dw 0xaa55