using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lorry
{
    public interface ILorryActivity
    {
        void SelectViewMode(Views mode);
        void ShowSpinner(bool state);
        void Alert(string text, string head);
    }
}