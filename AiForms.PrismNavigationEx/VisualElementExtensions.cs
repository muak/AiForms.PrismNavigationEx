using System;
using Xamarin.Forms;

namespace AiForms.PrismNavigationEx
{
    /// <summary>
    /// Reference: https://github.com/PrismLibrary/Prism/blob/411ff88c9fb38d382d5cbddc13649b011bda791e/src/Forms/Prism.Forms/Extensions/VisualElementExtensions.cs
    /// </summary>
    public static class VisualElementExtensions
    {
        public static bool TryGetParentPage(this VisualElement element, out Page page)
        {
            page = GetParentPage(element);
            return page != null;
        }

        private static Page GetParentPage(Element visualElement)
        {
            switch (visualElement.Parent)
            {
                case Page page:
                    return page;
                case null:
                    return null;
                default:
                    return GetParentPage(visualElement.Parent);
            }
        }
    }
}
