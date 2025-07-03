using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurila.Components;

public class ClassBuilder
{
    private readonly List<string> _items = [];

    public string Build()
    {
        return string.Join(" ", _items);
    }

    public ClassBuilder Add(string? item)
    {
        if (!string.IsNullOrWhiteSpace(item) && !_items.Contains(item))
        {
            _items.Add(item);
        }
        return this;
    }

    public ClassBuilder AddIf(bool condition, string? item)
    {
        if (condition)
        {
            Add(item);
        }
        return this;
    }

    public ClassBuilder AddIfElse(bool condition, string? ifItem, string? elseItem)
    {
        if (condition)
        {
            Add(ifItem);
        }
        else
        {
            Add(elseItem);
        }
        return this;
    }

    public ClassBuilder AddIfNot(bool condition, string? item)
    {
        if (!condition)
        {
            Add(item);
        }
        return this;
    }

    public ClassBuilder AddRange(IEnumerable<string> items)
    {
        foreach (var item in items)
        {
            Add(item);
        }
        return this;
    }
}
