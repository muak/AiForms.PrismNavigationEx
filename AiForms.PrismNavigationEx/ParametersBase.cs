using System;
using Prism.Navigation;

namespace AiForms.PrismNavigationEx
{
    public abstract class ParametersBase
    {
        internal const string ParameterKey = "aiforms.prismNavigationEx";
        /// <summary>
        /// 画面パラメータをPrism.Navigation.NavigationParametersに変換する
        /// </summary>
        /// <returns>The navigation parameters.</returns>
        public NavigationParameters ToNavigationParameters() {
            return new NavigationParameters{
                {ParameterKey,this}
            };
        }
    }
}
