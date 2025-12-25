using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

Console.WriteLine("Hello, World!");

string text = String.Create("Hello, World!".Length, "Hello, World!", static (Span<char> span, string state) =>
{
	for (int i = 0; i < state.Length; i++)
	{
		span[i] = state[i];
	}
});

Console.WriteLine($"Text: {text}");
Console.WriteLine($"ReferenceEquals: {ReferenceEquals(text, "Hello, World!")}");
text = String.Intern(text);
Console.WriteLine($"ReferenceEquals: {ReferenceEquals(text, "Hello, World!")}");
Console.WriteLine($"IsInterned: {String.IsInterned(text) is not null}");

ref char first = ref MemoryMarshal.GetReference(text.AsSpan());
ref char last = ref Unsafe.Add(ref first, text.Length - 1);
last = '?';

Console.WriteLine("Hello, World!");
