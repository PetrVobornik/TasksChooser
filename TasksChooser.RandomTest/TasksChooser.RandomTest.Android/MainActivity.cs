using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Android.Content.PM;
using Amporis.TasksChooser;
using System.IO;
using System;

namespace TasksChooser.RandomTest.Android
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        Button aButton;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            //SetContentView(Resource.Layout.activity_main);

            var layout = new LinearLayout(this);
            aButton = new Button(this);
            aButton.Text = "GO";
            aButton.Click += AButton_Click;
            layout.AddView(aButton);
            SetContentView(layout);
        }

        private void AButton_Click(object sender, System.EventArgs e)
        {
            string fileName = $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.bin";
            var path = Path.Combine(Application.Context.GetExternalFilesDir(null).AbsolutePath, fileName);

            RandomTester.CreateBinaryData(path);

            aButton.Text = path;
        }


        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}