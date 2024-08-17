using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Toucan.Compiler;
using Toucan.Modules.Callables;
using Toucan.Runtime;
using Toucan.Runtime.CodeGen;

namespace WpfThreadTest
{

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private GameObject gameObject;

    private ToucanVm vm = null;

    #region Public

    public MainWindow()
    {
        InitializeComponent();
        InitObjects();
    }

    #endregion

    #region Private

    private void Compile_OnClick( object sender, RoutedEventArgs e )
    {
        if ( vm != null )
        {
            vm.Stop();

            // wait for thread to exit?
            Task.Delay( 500 );
        }

        vm = new ToucanVm();
        vm.InitVm();
        vm.RegisterSystemModuleCallables();
        vm.SynchronizationContext = SynchronizationContext.Current;

        // Expose CSharp objects to the Toucan virtual machine
        vm.RegisterExternalGlobalObjects( new Dictionary < string, object > { { "gameObject", gameObject } } );

        ToucanCompiler compiler = new ToucanCompiler();

        try
        {
            ToucanProgram program = compiler.Compile( new[] { Code.Text } );

            vm.InterpretAsync( program, CancellationToken.None ).
               ContinueWith(
                   t =>
                   {
                       if ( t.IsFaulted )
                       {
                           Dispatcher.Invoke(
                               () =>
                               {
                                   MessageBox.Show(
                                       t.Exception.InnerException.Message,
                                       "Toucan WPF Thread Test",
                                       MessageBoxButton.OK,
                                       MessageBoxImage.Exclamation );
                               } );
                       }
                   } );
        }
        catch ( Exception exception )
        {
            MessageBox.Show(
                exception.Message,
                "Toucan WPF Thread Test",
                MessageBoxButton.OK,
                MessageBoxImage.Exclamation );
        }
    }

    private void InitObjects()
    {
        Ellipse circle = new Ellipse { Fill = Brushes.Black, Width = 100, Height = 100 };

        gameObject = new GameObject( circle );

        Canvas.Children.Add( circle );

        string mod = @"module Main;  

while ( true ) { 

    if (gameObject.X < 0 || gameObject.X > 300 ) {
        gameObject.dX = -gameObject.dX;
    }

    gameObject.X += gameObject.dX;

    sync {
        gameObject.Move();
    }

}";

        Code.Text = mod;
    }

    private void Stop_OnClick( object sender, RoutedEventArgs e )
    {
        if ( vm != null )
        {
            vm.Stop();

            // wait for thread to exit?
            Task.Delay( 500 );
        }
    }

    #endregion
}

}
