using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prism.Behaviors;
using Prism.Common;
using Prism.Ioc;
using Prism.Navigation;
using Xamarin.Forms;

namespace AiForms.PrismNavigationEx
{    
    public class PageNavigationServiceEx:PageNavigationService,INavigationServiceEx
    {
        public IContainerExtension Container { get;private set; }
        IApplicationProvider _app;
        public const string PageNavigationServiceExName = "AiForms.NavigationService";
        public const string NavigationModeKey = "__NavigationMode";
        internal static NavigationParameters ParameterProxy { get; set;}

        public PageNavigationServiceEx(IContainerExtension container, IApplicationProvider applicationProvider,IPageBehaviorFactory pageBehaviorFactory) 
            :base(container,applicationProvider,pageBehaviorFactory)
        {
            _app = applicationProvider;
            Container = container;
        }

        public Page MainPage { 
            get {
                return _app.MainPage;
            } 
        }

        public void SetAutowireViewModelOnPage(Page page) {
            var vmlResult = Prism.Mvvm.ViewModelLocator.GetAutowireViewModel(page);
            if (vmlResult == null)
                Prism.Mvvm.ViewModelLocator.SetAutowireViewModel(page, true);
        }

        public TabbedPage CreateMainPageTabbedHasNavigation(string tabbedName, IEnumerable<NavigationPage> children) {
            var tabbedPage = CreatePage(tabbedName) as TabbedPage; 

            SetAutowireViewModelOnPage(tabbedPage);

            foreach (var c in children) {
                tabbedPage.Children.Add(c);
                c.Behaviors.Add(new TabbedPageHasNavigationPageActionBehavior());
            }

            //子を追加し終わってからBehaviorを適用しないとActiveAwareが余分に呼ばれる
            _pageBehaviorFactory.ApplyPageBehaviors(tabbedPage);

            return tabbedPage;
        }

        public NavigationPage CreateNavigationPage(string navName,string pageName,NavigationParameters parameters=null){

            var naviPage = CreatePageFromSegment(navName) as NavigationPage;
            naviPage.Behaviors.Remove(naviPage.Behaviors.FirstOrDefault(x => x.GetType() == typeof(NavigationPageActiveAwareBehavior)));

            var contentPage = CreatePageFromSegment(pageName);

            if (parameters == null) {
                parameters = new NavigationParameters();
            }
            ((INavigationParametersInternal)parameters).Add(NavigationModeKey,NavigationMode.New);

            PageUtilities.OnInitializedAsync(contentPage,parameters);

            naviPage.PushAsync(contentPage, false).Wait();

            PageUtilities.OnNavigatedTo(contentPage,parameters);

            return naviPage;
        }

        public ContentPage CreateContentPage(string pageName) {
            var contentPage = CreatePageFromSegment(pageName);

            return contentPage as ContentPage;
        }

        public Task<INavigationResult> GoBackAsync(NavigationParameters parameters, bool? useModalNavigation, bool animated = true) {
            //GoBackAsyncの時にTabbedPageのCurrentPageにParameterProxyを通して渡す。実行はBehaviorで行う
            PageNavigationServiceEx.ParameterProxy = parameters;
            return GoBackInternal(parameters, useModalNavigation, animated);
        }

        public Task NavigateAsync(string name, NavigationParameters parameters, bool? useModalNavigation, bool animated = true) {
            return NavigateInternal(name, parameters, useModalNavigation, animated);
        }

        public async Task Navigate<T>(ParametersBase parameters = null, bool animated = true) where T : ContentPage {

            var prismParam = parameters?.ToNavigationParameters();
            if (prismParam == null) {
                prismParam = new NavigationParameters();
            }
            await NavigateAsync(typeof(T).Name, prismParam, (bool?)false, animated);
        }

        public async Task NavigateModal<T>(ParametersBase parameters = null, bool animated = true) where T : ContentPage {
            var prismParam = parameters?.ToNavigationParameters();
            if (prismParam == null) {
                prismParam = new NavigationParameters();
            }
            await NavigateAsync(typeof(T).Name, prismParam, (bool?)true, animated);
        }

        public async Task NavigateModal<Tnavi, Tpage>(ParametersBase parameters = null, bool animated = true)
            where Tnavi : NavigationPage
            where Tpage : ContentPage
        {
            var prismParam = parameters?.ToNavigationParameters();
            if (prismParam == null) {
                prismParam = new NavigationParameters();
            }
            await NavigateAsync(typeof(Tnavi).Name + "/" + typeof(Tpage).Name, prismParam, (bool?)true, animated);
        }

        public async Task GoBackModalAsync(bool animated = true)
        {
            await GoBackAsync(null, true, animated);
        }

        public async Task AnywhereNavigate<T>(ParametersBase parameters = null, bool animated = true) where T : ContentPage
        {
            var navi = GetNavigationCurrentPage(_app.MainPage);

            var page = CreatePage(typeof(T).Name);

            var prismParam = parameters?.ToNavigationParameters();
            if (prismParam == null)
            {
                prismParam = new NavigationParameters();
            }
            ((INavigationParametersInternal)prismParam)?.Add(NavigationModeKey, NavigationMode.New);

            await PageUtilities.OnInitializedAsync(page, prismParam);

            await navi.Navigation.PushAsync(page, animated);

            PageUtilities.OnNavigatedTo(page, prismParam);
        }

        public async Task AnywhereNavigateModal<Tnavi,Tpage>(ParametersBase parameters = null, bool animated = true)
            where Tnavi : NavigationPage
            where Tpage : ContentPage
        {
            var navigation = _app.MainPage.Navigation;

            var naviPage = CreatePage(typeof(Tnavi).Name) as NavigationPage;
            naviPage.Behaviors.Remove(naviPage.Behaviors.FirstOrDefault(x => x.GetType() == typeof(NavigationPageActiveAwareBehavior)));

            var page = CreatePage(typeof(Tpage).Name);

            var prismParam = parameters?.ToNavigationParameters();
            if (prismParam == null)
            {
                prismParam = new NavigationParameters();
            }
            ((INavigationParametersInternal)prismParam)?.Add(NavigationModeKey, NavigationMode.New);           

            await PageUtilities.OnInitializedAsync(page, prismParam);
            await naviPage.Navigation.PushAsync(page,false);
            PageUtilities.OnNavigatedTo(page, prismParam);

            naviPage.Navigation.RemovePage(naviPage.RootPage);

            await PageUtilities.OnInitializedAsync(naviPage, prismParam);
            await navigation.PushModalAsync(naviPage,animated);
            PageUtilities.OnNavigatedTo(naviPage, prismParam);
        }

        public bool ChangeTab<T>() where T : Page
        {
            var mainPage = this.MainPage;
            if (mainPage == null) {
                return false;
            }

            TabbedPage tabbed = null;
            if (mainPage is TabbedPage) {
                tabbed = mainPage as TabbedPage;
                return SearchTargetTab(tabbed, typeof(T));
            }
            else if (mainPage is NavigationPage && (mainPage as NavigationPage)?.CurrentPage is TabbedPage) {
                tabbed = (mainPage as NavigationPage).CurrentPage as TabbedPage;
                return SearchTargetTab(tabbed, typeof(T));
            }
            else {
                return false;
            }

        }

        bool SearchTargetTab(TabbedPage tabbed, Type target)
        {

            foreach (var child in tabbed.Children) {
                if (child.GetType() == target) {
                    tabbed.CurrentPage = child;
                    return true;
                }
                var nav = (child as NavigationPage);
                if (nav == null) {
                    continue;
                }

                if (nav.CurrentPage.GetType() == target) {
                    tabbed.CurrentPage = child;
                    return true;
                }
            }

            return false;
        }

        Page GetNavigationCurrentPage(Page page)
        {
            if (page is NavigationPage)
            {
                return (page as NavigationPage).CurrentPage;
            }
            else if (page is FlyoutPage)
            {
                return GetNavigationCurrentPage((page as FlyoutPage).Detail);
            }
            else if (page is TabbedPage)
            {
                return GetNavigationCurrentPage((page as TabbedPage).CurrentPage);
            }

            return null;
        }
    }
}
