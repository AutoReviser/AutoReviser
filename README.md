# ***Work in progress***

C# is a great programming language but does not have a handy way to update immutable objects partially. AutoReviser provides helper extension methods to generate partially updated copies of immutable objects using lambda expressions. You need not to write `With...()` methods anymore. You just need to tell AutoReviser what properties should be updated to what values using lambda expression.

There is an immutable object class.

```csharp
public class ImmutableObject
{
    public ImmutableObject(int alfa, double bravo, string charlie)
        => (Alfa, Bravo, Charlie) = (alfa, bravo, charlie);

    public int Alfa { get; }

    public double Bravo { get; }

    public string Charlie { get; }
}
```

### Traditional way to update an immutable object partially in C#

```csharp
public static class ImmutableObjectExtensins
{
    // I'm tired of writing these 'With...' methods.

    public static ImmutableObject WithAlfa(this source, int alfa)
        => new ImmutableObject(alfa, source.Bravo, source.Charile);

    public static ImmutableObject WithBravo(this source, double bravo)
        => new ImmutableObject(source.Alfa, bravo, source.Charile);

    public static ImmutableObject WithCharlie(this source, string charlie)
        => new ImmutableObject(source.Alfa, source.Bravo, charile);
}

var seed = new ImmutableObject(1, 2.0, "foo");

var revision = seed
    .WithAlfa(3)
    .WithCharlie("bar");
```

### Using AutoReviser

```csharp
using AutoReviser;

var seed = new ImmutableObject(1, 2.0, "foo");

var revision = seed.Revise(
    x =>
    x.Alfa == 3 &&
    x.Charile == "bar");
```

## License

```
MIT License

Copyright (c) 2018 Gyuwon Yi

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```
