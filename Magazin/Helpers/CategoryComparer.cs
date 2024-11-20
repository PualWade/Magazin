using System;
using System.Collections.Generic;
using Magazin.Models;

namespace Magazin.Helpers
{
    public class CategoryComparer : IEqualityComparer<Category>
    {
        public bool Equals(Category x, Category y)
        {
            return string.Equals(x.CategoryName, y.CategoryName, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(Category obj)
        {
            return obj.CategoryName.ToLowerInvariant().GetHashCode();
        }
    }
}
