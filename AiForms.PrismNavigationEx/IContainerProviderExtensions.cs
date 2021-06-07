using System;
using Prism.Common;
using Prism.Ioc;
using Xamarin.Forms;

namespace AiForms.PrismNavigationEx
{
    public static class IContainerProviderExtensions
    {
        public static INavigationServiceEx CreateNavigationService(this IContainerProvider container, Page page)
        {
            var navigationService = container.Resolve<INavigationServiceEx>(PageNavigationServiceEx.PageNavigationServiceExName);
            ((IPageAware)navigationService).Page = page;
            return navigationService;
        }
    }
}
