# String Interning

> [!NOTE]
> This blog post is part of the **C# Advent Calendar 2025** series. See [csadvent.christmas][csharp-advent] for the entire Advent.

## Index
- [Remarks](#remarks)
- [String Intern Pool](#string-intern-pool)
- [Summary](#summary)
- [Would you like to know more?](#would-you-like-to-know-more)
- [Metadata](#metadata)

## Remarks

```CSharp
Console.WriteLine("Hello, World!");
```

This is my very first blog post ever.
I've been procrastinating this endeavor forever.
Now the [C# Advent][csharp-advent] is the perfect opportunity to delay no more.
But what should I be writing about?
Well, perhaps about the `"Hello, World!"` [String][docs-dotnet-api-system-string] of the C# _Console App_ template:
Let's discuss _String Interning_!

## String Intern Pool

By default, [Roslyn][github-dotnet-roslyn] (the .NET Compiler Platform) automatically interns [string literals][docs-dotnet-csharp-strings] into the [Common Language Runtime][docs-dotnet-clr] (CLR) intern pool.
So the same C# _string literal_ (case-sensitive) is a reference to the unique UTF-16 code unit in memory:
```CSharp
Console.WriteLine(Object.ReferenceEquals("Text", "Text")); // True
Console.WriteLine(Object.ReferenceEquals("text", "Text")); // False
```

With the API [System.String.IsInterned(String)][docs-dotnet-api-system-string-isinterned] we can retrieve a reference to the [String][docs-dotnet-api-system-string] in the CLR intern pool (if available, `null` otherwise):
```CSharp
Console.WriteLine($"IsInterned: {String.IsInterned("Text") is not null}"); // IsInterned: True
```

Strings assembled otherwise are not interned automatically:
```CSharp
string text = new StringBuilder("Hello").Append(',').Append(' ').Append("World").Append('!').ToString();
Console.WriteLine($"IsInterned: {String.IsInterned(text) is not null}"); // IsInterned: False
```

But we may intern strings manually via the API [System.String.Intern(String)][docs-dotnet-api-system-string-intern]:
```CSharp
string text = new StringBuilder("Hello").Append(',').Append(' ').Append("World").Append('!').ToString();
Console.WriteLine($"IsInterned: {String.IsInterned(text) is not null}"); // IsInterned: False
string interned = String.Intern(text);
Console.WriteLine($"IsInterned: {String.IsInterned(interned) is not null}"); // IsInterned: True
```

Now let's prove _string interning_ in an unorthodox fashion.
Although references to [String][docs-dotnet-api-system-string] (and likewise the synonymous `string` C# _keyword_) are read-only, the [String][docs-dotnet-api-system-string] in memory itself is not immutable.
With a bit of _unsafe_ code, we can get a non-readonly `ref` to the first [Char][docs-dotnet-api-system-char] of the [String][docs-dotnet-api-system-string], which we may overwrite.
Then, with _pointer arithmetic_, we can overwrite all `char`s in the `string`.
Ultimately, due to the original _string literal_ being interned, the referenced location in memory (intern pool) now contains different _characters_.
```CSharp
string text = "Hello, World!";
Console.WriteLine(text); // Hello, World!
Console.WriteLine(text.Length); // 13

ref char first = ref MemoryMarshal.GetReference(text.AsSpan());

first = 'C';
Unsafe.Add(ref first, 1) = '#';
Unsafe.Add(ref first, 2) = 'A';
Unsafe.Add(ref first, 3) = 'd';
Unsafe.Add(ref first, 4) = 'v';
Unsafe.Add(ref first, 5) = 'e';
Unsafe.Add(ref first, 6) = 'n';
Unsafe.Add(ref first, 7) = 't';
Unsafe.Add(ref first, 8) = ' ';
Unsafe.Add(ref first, 9) = '2';
Unsafe.Add(ref first, 10) = '0';
Unsafe.Add(ref first, 11) = '2';
Unsafe.Add(ref first, 12) = '5';

Console.WriteLine(text); // C#Advent 2025
Console.WriteLine("Hello, World!"); // C#Advent 2025
```

With [System.Runtime.InteropServices.MemoryMarshal.GetReference<T>(ReadOnlySpan<T>)][docs-dotnet-api-system-runtime-interopservices-memorymarshal-getreference] we receive the reference to the element of the `Span<T>` or `ReadOnlySpan<T>` at index 0, hence the first _character_ in the _string_.

And with [System.Runtime.CompilerServices.Unsafe.Add<T>(T, Int32)][docs-dotnet-api-system-runtime-compilerservices-unsafe-add] we receive the reference at the specified offset.
This API is _unsafe_, as we can get a hold of a _managed pointer_ (or an _unmanaged pointer_) outside the range of the contiguous memory that the `Span<T>`/`ReadOnlySpan<T>` represents, ending up reinterpreting memory, leading to undefined behavior.
Or, more fatally , when trying to access memory outside of our process's address space, causing a _segmentation fault_ ([System.AccessViolationException][docs-dotnet-api-system-accessviolationexception]).

## Summary

Playing some shenanigans with C# and the .NET Runtime, I showcased _String Interning_ of the Compiler and the CLR.
Speaking of `unsafe`, the _Working Set_ for _C# 15.0_ includes the [evolution of `unsafe`][github-dotnet-csharplang-unsafe-evolution], but that's another post for another time.

## Would you like to know more?
- [C# 14.0: String literals in data section as UTF-8 (experimental feature)][github-dotnet-roslyn-string-literals-data-section]

## Metadata
- Blog post **#1**
- Tags: dotnet;csharp;runtime;string;interning;unsafe;marshal;memory
- Example: [StringInterning](./../code/String-Interning/)
- Discussion: [String Interning #1](https://github.com/Flash0ver/dotnet-in-a-flash/discussions/1)
- History
  - **2025-12-27**: various fixes, additional links and resources, revise demo project, add _GitHub Discussion_
  - **2025-12-25**: Initial publish

[`goto` top](#string-interning) | [Edit this page](https://github.com/Flash0ver/dotnet-in-a-flash/edit/main/blog/String-Interning.md) (fixes and improvements are appreciated) | [Create a new issue](https://github.com/Flash0ver/dotnet-in-a-flash/issues/new?title=fix(string-interning):&body=Describe+the+problem+with+the+blog+post:+_String-Interning_)

[csharp-advent]: https://csadvent.christmas/
[github-dotnet-roslyn]: https://github.com/dotnet/roslyn
[github-dotnet-roslyn-string-literals-data-section]: https://github.com/dotnet/roslyn/blob/main/docs/features/string-literals-data-section.md
[github-dotnet-csharplang-unsafe-evolution]: https://github.com/dotnet/csharplang/issues/9704
[docs-dotnet-clr]: https://learn.microsoft.com/dotnet/standard/clr/?wt.mc_id=DT-MVP-5005026
[docs-dotnet-csharp-strings]: https://learn.microsoft.com/dotnet/csharp/programming-guide/strings/?wt.mc_id=DT-MVP-5005026
[docs-dotnet-api-system-string]: https://learn.microsoft.com/dotnet/api/System.String/?wt.mc_id=DT-MVP-5005026
[docs-dotnet-api-system-string-isinterned]: https://learn.microsoft.com/dotnet/api/System.String.IsInterned/?wt.mc_id=DT-MVP-5005026
[docs-dotnet-api-system-string-intern]: https://learn.microsoft.com/dotnet/api/system.string.intern/?wt.mc_id=DT-MVP-5005026
[docs-dotnet-api-system-char]: https://learn.microsoft.com/dotnet/api/system.char/?wt.mc_id=DT-MVP-5005026
[docs-dotnet-api-system-runtime-interopservices-memorymarshal-getreference]: https://learn.microsoft.com/dotnet/api/System.Runtime.InteropServices.MemoryMarshal.GetReference/?wt.mc_id=DT-MVP-5005026
[docs-dotnet-api-system-runtime-compilerservices-unsafe-add]: https://learn.microsoft.com/dotnet/api/System.Runtime.CompilerServices.Unsafe.Add/?wt.mc_id=DT-MVP-5005026
[docs-dotnet-api-system-accessviolationexception]: https://learn.microsoft.com/en-us/dotnet/api/System.AccessViolationException/?wt.mc_id=DT-MVP-5005026
