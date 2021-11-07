using System;

using SatisfactoryModdingHelper.Models;

namespace SatisfactoryModdingHelper.Contracts.Services
{
    public interface IThemeSelectorService
    {
        void InitializeTheme();

        void SetTheme(AppTheme theme);

        AppTheme GetCurrentTheme();
    }
}
