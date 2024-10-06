using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading.Tasks;
using TongTongAdmin.UWP.Helpers;
using Windows.Storage;

namespace TongTongAdmin.UWP.Services
{
    public class UwpSuspensionDriver<TAppState> : ISuspensionDriver where TAppState : class
    {
        const string appStateKey = "appState";

        public IObservable<Unit> SaveState(object state)
        {
            throw new NotImplementedException();

            //var suspensionState = new SuspensionState()
            //{
            //    SuspensionDate = DateTime.Now
            //};

            //var target = OnBackgroundEntering?.Target.GetType();
            //var onBackgroundEnteringArgs = new OnBackgroundEnteringEventArgs(suspensionState, target);

            //OnBackgroundEntering?.Invoke(this, onBackgroundEnteringArgs);

            //return ApplicationData.Current.LocalFolder.SaveAsync(appStateKey, onBackgroundEnteringArgs)
            //    .ToObservable();
        }

        public IObservable<object> LoadState()
        {
            throw new NotImplementedException();

            //var saveState = ApplicationData.Current.LocalFolder.ReadAsync<OnBackgroundEnteringEventArgs>(StateFilename)
            //    .ToObservable();

            //if(saveState?.Target != null)
            //{
            //    var navigationService = ServiceLocator.Current.GetInstance<NavigationServiceEx>();
            //    navigationService.Navigate(saveState.Target.FullName, saveState.SuspensionState);
            //}
        }

        public IObservable<Unit> InvalidateState()
        {
            return ApplicationData.Current.LocalFolder.DeleteAsync(appStateKey)
                .ToObservable();
        }
    }
}
