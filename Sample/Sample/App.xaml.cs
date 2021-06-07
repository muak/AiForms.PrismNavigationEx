using System;
using AiForms.PrismNavigationEx;
using Prism;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Mvvm;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Sample
{
    public partial class App : PrismApplication
    {
        public App(IPlatformInitializer initializer = null): base(initializer)
        {
            InitializeComponent();
        }

        protected override void OnInitialized()
        {
            var navi = (PageNavigationServiceEx)Container.Resolve<INavigationServiceEx>(PageNavigationServiceEx.PageNavigationServiceExName);

            MainPage = navi.CreateMainPageTabbedHasNavigation()
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            throw new NotImplementedException();
        }

        protected override void RegisterRequiredTypes(IContainerRegistry containerRegistry)
        {
            base.RegisterRequiredTypes(containerRegistry);
            containerRegistry.Register<INavigationServiceEx, PageNavigationServiceEx>();    // VM以外でDIするために必要
            containerRegistry.Register<INavigationServiceEx, PageNavigationServiceEx>(PageNavigationServiceEx.PageNavigationServiceExName);
        }

        protected override void ConfigureViewModelLocator()
        {
            ViewModelLocationProvider.SetDefaultViewModelFactory((view, type) =>
            {

                INavigationServiceEx navigationService = null;
                switch (view)
                {
                    case Page page:
                        navigationService = Container.CreateNavigationService(page);
                        break;
                    case VisualElement visualElement:
                        if (visualElement.TryGetParentPage(out var attachedPage))
                        {
                            navigationService = Container.CreateNavigationService(attachedPage);
                        }
                        break;
                }

                return Container.Resolve(type, (typeof(INavigationServiceEx), navigationService));
            });
        }



        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
