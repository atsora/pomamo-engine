
/// Rule main
val main: lexbuf: LexBuffer<char> -> token
/// Rule comment
val comment: lexbuf: LexBuffer<char> -> token
/// Rule lastarg
val lastarg: command: obj -> args: obj -> lexbuf: LexBuffer<char> -> token
/// Rule nargs
val nargs: command: obj -> args: obj -> n: obj -> lexbuf: LexBuffer<char> -> token
/// Rule multipleargs
val multipleargs: command: obj -> args: obj -> lexbuf: LexBuffer<char> -> token
/// Rule mcommand
val mcommand: mcode: obj -> args: obj -> lexbuf: LexBuffer<char> -> token
/// Rule read_quoted_string
val read_quoted_string: s: obj -> lexbuf: LexBuffer<char> -> token
