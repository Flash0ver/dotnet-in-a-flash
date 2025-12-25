# String Interning
- Initial publish: **2025-12-25**
- Tags: dotnet;csharp;runtime;string;interning;unsafe;marshal;memory
- Example: [StringInterning](./../code/String-Interning/)

> [!NOTE]
> This blog post is part of the **C# Advent Calendar 2025** series. See [csadvent.christmas][csharp-advent] for the the entire advent.

# Index
- [Remarks](#remarks)
- [String Intern Pool](#string-intern-pool)
- [Summary](#summary)

## Remarks

```CSharp
Console.WriteLine("Hello, World!");
```

This is my very first blog post ever.
I've been procrastinating this endeavor forever.
Now the [C# Advent][csharp-advent] is the perfect opportunity to delay no more.
But what should I be writing about?
Well, perhaps about the `"Hello, World!"` [String][docs-dotnet-api-system-string] C# _Console App_ template:
Let's discuss

## String Intern Pool

Per default, the [Roslyn][github-dotnet-roslyn] (the .NET Compiler Platform) automatically interns [string literals][docs-dotnet-csharp-strings] into the [Common Language Runtime][docs-dotnet-clr] (CLR) intern pool.
So the same C# _string literal_ (case-sensitive) is a reference to the unique UTF-16 code unit in memory:
```CSharp
Console.WriteLine(Object.ReferenceEquals("Text", "Text")); // True
Console.WriteLine(Object.ReferenceEquals("text", "Text")); // False
```

With the API [System.String.IsInterned(String)][docs-dotnet-api-system-string-isinterned] we can retrieve a reference to the [String][docs-dotnet-api-system-string] in the CLR intern pool:
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
Ultimately, despite the original _string_ still being interned, it now contains different _characters_.

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

## Summary

Playing some shenanigans with C# and the .NET Runtime, I showcased _String Interning_ of the Compiler and the CLR.

[csharp-advent]: https://csadvent.christmas/
[github-dotnet-roslyn]: https://github.com/dotnet/roslyn
[docs-dotnet-clr]: https://learn.microsoft.com/dotnet/standard/clr/?wt.mc_id=DT-MVP-5005026
[docs-dotnet-csharp-strings]: https://learn.microsoft.com/dotnet/csharp/programming-guide/strings/?wt.mc_id=DT-MVP-5005026
[docs-dotnet-api-system-string]: https://learn.microsoft.com/dotnet/api/System.String/?wt.mc_id=DT-MVP-5005026
[docs-dotnet-api-system-string-isinterned]: https://learn.microsoft.com/dotnet/api/System.String.IsInterned/?wt.mc_id=DT-MVP-5005026
[docs-dotnet-api-system-string-intern]: https://learn.microsoft.com/dotnet/api/system.string.intern/?wt.mc_id=DT-MVP-5005026
[docs-dotnet-api-system-char]: https://learn.microsoft.com/dotnet/api/system.char/?wt.mc_id=DT-MVP-5005026
