using System;
using Prism;
using Prism.Behaviors;
using Prism.Common;
using Xamarin.Forms;

namespace AiForms.PrismNavigationEx
{
    /// <summary>
    /// TabbedPage/NavigationPage/ContentPageの構成で
    /// ContentPageから次ページへ遷移したときにActiveAwareの状態を引き継ぎ
    /// 次ページから戻ってきた時に元のページをActive化するためBehavior
    /// 
    /// 本家
    /// https://github.com/PrismLibrary/Prism/blob/7.0.0-Forms-SR1/Source/Xamarin/Prism.Forms/Behaviors/NavigationPageActiveAwareBehavior.cs
    /// こちらは使わず、ページ遷移時に遷移元ページを非アクティブにするロジックだけを使う
    /// </summary>
    public class TabbedPageHasNavigationPageActionBehavior: BehaviorBase<NavigationPage>
    {
        protected override void OnAttachedTo(NavigationPage bindable) {
            base.OnAttachedTo(bindable);
            // PropertyChanging/Changedだけの判定の方が正確っぽい
            //bindable.Pushed += Bindable_Pushed;
            //bindable.Popped += Bindable_Popped;
            bindable.PropertyChanging += NavigationPage_PropertyChanging;
            bindable.PropertyChanged += NavigationPage_PropertyChanged;
        }

        

        protected override void OnDetachingFrom(NavigationPage bindable) {
            base.OnDetachingFrom(bindable);
            //bindable.Pushed -= Bindable_Pushed;
            //bindable.Popped -= Bindable_Popped;
            bindable.PropertyChanging -= NavigationPage_PropertyChanging;
            bindable.PropertyChanged -= NavigationPage_PropertyChanged;
        }

        void Bindable_Pushed(object sender, NavigationEventArgs e) {
            SetIsActive(e.Page,true);
        }

        void Bindable_Popped(object sender, NavigationEventArgs e) {
            SetIsActive(e.Page, false);
            SetIsActive(AssociatedObject.CurrentPage, true);
        }

        void NavigationPage_PropertyChanging(object sender, PropertyChangingEventArgs e) {
            if (e.PropertyName == "CurrentPage") {
                SetIsActive(AssociatedObject.CurrentPage, false);
            }
        }

        void NavigationPage_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentPage")
            {
                SetIsActive(AssociatedObject.CurrentPage, true);
            }
        }


        void SetIsActive(object view, bool isActive) {
            PageUtilities.InvokeViewAndViewModelAction<IActiveAware>(view, activeAware => activeAware.IsActive = isActive);
        }
    }
}
