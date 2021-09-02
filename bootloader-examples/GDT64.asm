; GLOBAL DESCRIPTOR TABLE FOR 64 BIT MODE

; sources
; https://github.com/sedflix/lame_bootloader/
; https://wiki.osdev.org/Setting_Up_Long_Mode

GDT64:
    .Null: equ $ - GDT64
    dw 0xFFFF
    dw 0
    db 0
    db 0
    db 1
    db 0
    .Code: equ $ - GDT64
    dw 0
    dw 0
    db 0
    db 10011010b         
    db 10101111b         
    db 0                 
    .Data: equ $ - GDT64 
    dw 0                 
    dw 0                 
    db 0                 
    db 10010010b         
    db 00000000b         
    db 0                 
    .Pointer:            
    dw $ - GDT64 - 1     
    dq GDT64