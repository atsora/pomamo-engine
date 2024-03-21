
/// Rule main
val main: lexbuf: LexBuffer<char> -> token
/// Rule comment
val comment: lexbuf: LexBuffer<char> -> token
/// Rule comment2
val comment2: lexbuf: LexBuffer<char> -> token
/// Rule extra
val extra: lexbuf: LexBuffer<char> -> token
/// Rule file
val file: lexbuf: LexBuffer<char> -> token
/// Rule dprntrule
val dprntrule: lexbuf: LexBuffer<char> -> token
