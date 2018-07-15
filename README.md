# AutoRevisor

[![Build status](https://ci.appveyor.com/api/projects/status/n9tskki80p2g6ahk/branch/master?svg=true)](https://ci.appveyor.com/project/gyuwon/autoreviser/branch/master)

C# is a great programming language but does not have a handy way to update immutable objects partially. AutoReviser provides helper extension methods to generate partially updated copies of immutable objects using lambda expressions. You need not to write `With...()` methods anymore. You just need to tell AutoReviser what properties should be updated to what values using lambda expression.

## How to use

There are immutable object classes.

```csharp
public class ImmutableObject
{
    public ImmutableObject(int alfa, string bravo)
        => (Alfa, Bravo) = (alfa, bravo);

    public int Alfa { get; }

    public string Bravo { get; }
}

public class ComplexImmutableObject
{
    public ComplexImmutableObject(int charile, ImmutableObject delta)
        => (Charlie, Delta) = (charlie, delta);

    public int Charlie { get; }

    public ImmutableObject Delta { get; }
}
```

### Traditional way to update an immutable object partially in C#

```csharp
public static class ImmutableObjectExtensins
{
    // I'm tired of writing these 'With...' methods.

    public static ImmutableObject WithAlfa(this source, int alfa)
    {
        return new ImmutableObject(alfa, source.Bravo);
    }

    public static ImmutableObject WithBravo(this source, string bravo)
    {
        return new ImmutableObject(source.Alfa, bravo);
    }

    public static ComplexImmutableObject WithCharlie(this source, int charlie)
    {
        return new ComplexImmutableObject(charlie, source.Delta);
    }

    public static ComplexImmutableObject WithDelta(
        this source, ImmutableObject delta)
    {
        return new ComplexImmutableObject(source.Charlie, delta);
    }
}

var source = new ComplexImmutableObject(1, new ImmutableObject(2, "foo"));

var revision = source
    .WithCharlie(10)
    .WithDelta(source.Delta.WithBravo("foo"));
```

### Using AutoReviser

```csharp
using AutoReviser;

var source = new ComplexImmutableObject(1, new ImmutableObject(2, "foo"));

var revision = source.Revise(
    x =>
    x.Charlie == 10 &&
    x.Delta.Bravo == "foo");
```

## Install package

```text
PM> Install-Package AutoReviser
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
