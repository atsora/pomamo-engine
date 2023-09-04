module Lemoine.Patterns

let (|Prefix|_|) (p:string) (s:string) =
  if s.StartsWith (p) then
    Some(s.Substring(p.Length))
  else
    None

let (|Split2|_|) (sep:string) (s:string) =
  let separators = sep.ToCharArray () in
  match s.Split (separators, 2) with
  | [| a; b |] -> Some(a, b)
  | _ -> None

let (|SingleChar|_|) (s:string) =
  if s.Length = 1 then
    Some(s[0])
  else
    None
