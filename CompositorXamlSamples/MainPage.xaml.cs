using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace CompositorXamlSamples
{
    public sealed partial class MainPage : Page
    {

        ContainerVisual _bottomVisual;
        Compositor _compositor;
        CompositionImage _largeImage;
        
        Microsoft.Graphics.Canvas.Effects.SaturationEffect _saturationDefinition;
        CompositionEffect _saturationEffect;
        EffectVisual _saturationVisual;


        public MainPage()
        {
            this.InitializeComponent();

            App.Current.Suspending += (o,e) => {
                try {
                    CleanUp();
                }
                catch (Exception ex) {
                    
                }
                
            };
        }

        
        private void CleanUp() {
            
            if (_bottomVisual != null) {
                _bottomVisual.Children.RemoveAll();

                _bottomVisual.Dispose();
                _compositor.Dispose();
                _largeImage.Dispose();
                _saturationDefinition.Dispose();
                _saturationEffect.Dispose();
                _saturationVisual.Dispose();
            }
        }

        private void layoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            if (_bottomVisual == null)
            {
                _bottomVisual = (ContainerVisual)ElementCompositionPreview.GetContainerVisual(bottomSurface);
                _compositor = _bottomVisual.Compositor;
            }


            InitCompositionBits();

        }

        private async void InitCompositionBits() {

            if (_largeImage != null) _largeImage.Dispose();
            _largeImage = _compositor.DefaultGraphicsDevice.CreateImageFromUri(new Uri("ms-appx:///Assets/Flower2.jpg"));

            await _largeImage.CompleteLoadAsync();


            //_imgVisual = _compositor.CreateImageVisual();
            //_imgVisual.Image = _largeImage;

            //_imgVisual.Size = new System.Numerics.Vector2((float)_largeImage.Size.Width, (float)_largeImage.Size.Height);
            //_imgVisual.Offset = new System.Numerics.Vector3((float)75, (float)75, 0);
            ////_imgVisual.Scale = new System.Numerics.Vector3(0.5f, 0.5f, 0);
            //_bottomVisual.Children.InsertAtTop(_imgVisual);



            //doing saturation
            _saturationDefinition = new Microsoft.Graphics.Canvas.Effects.SaturationEffect();
            _saturationDefinition.Saturation = 1f;
            _saturationDefinition.Name = "sat";
            _saturationDefinition.Source = new CompositionEffectSourceParameter("Image");

            CompositionEffectFactory cef = _compositor.CreateEffectFactory(_saturationDefinition);

            _saturationEffect = cef.CreateEffect();
            _saturationEffect.SetSourceParameter("Image", _largeImage);

            _saturationVisual = _compositor.CreateEffectVisual();
            _saturationVisual.Effect = _saturationEffect;
            _saturationVisual.Size = new System.Numerics.Vector2((float)_largeImage.Size.Width, (float)_largeImage.Size.Height);
            _saturationVisual.Offset = new System.Numerics.Vector3((float)75, (float)75, 0);
            _saturationVisual.Scale = new System.Numerics.Vector3(0.4f, 0.4f, 0);

            _bottomVisual.Children.InsertAtBottom(_saturationVisual);
            
        }
        
        private void sliderSaturation_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_saturationDefinition == null) return;
            _saturationDefinition.Saturation = (float)e.NewValue;


            CompositionEffectFactory cef = _compositor.CreateEffectFactory(_saturationDefinition);
            _saturationEffect = cef.CreateEffect();
            _saturationEffect.SetSourceParameter("Image", _largeImage);
            _saturationVisual.Effect = _saturationEffect;
        }
    }
}
