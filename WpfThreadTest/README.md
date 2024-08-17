# WpfThreadTest

**NOTE:** While this demo uses WPF to show cross-thread object access, the same principles can be applied to Unity.

The main purpose of this project is to 

* show Toucan in action inside an acutal application
* demonstrate the ability to compile a Toucan program and run on the fly
* demonstrate how to manipulate objects in your C# program from within your Toucan program
* most importantly, **show how to access objects created on another thread, such as when updating UIElements in WPF, or game objects in Unity**.

# A simple Toucan program

This simple program sets up an infinite loop and causes the object to move left and right.

```Toucan 
module Main;

while ( true ) { 
    if (gameObject.X < 0 || gameObject.X > 300 ) {
        gameObject.dX = -gameObject.dX;
    }
    gameObject.X += gameObject.dX;

    gameObject.Move();
}
```

`gameObject` is an object created in the host program that we pass in to Toucan using the `ToucanVm`'s `RegisterExternalGlobalObjects` method.

# The GameObject

We use the WPF `Canvas` as a simple rendering engine.  We add a `UIElement` and we want to animate it using Toucan. WPF uses static methods (e.g. `Canvas.SetLeft`) to affect the `UIElement`'s position.

It's possible but awkward to pass in the static Canvas class into Toucan to perform the actual movement, so instead we wrap our `UIElement` with the the `GameObject` class, which allows us to "redirect" access to the Canvas static through more accessible instance methods.

```cs
public class GameObject
{
    private readonly UIElement m_Element;

    public float X { get; set; }

    public float Y { get; set; }

    public float dX { get; set; }

    public float dY { get; set; }


    public GameObject( UIElement element )
    {
        m_Element = element;
        dX = 0.01f;
        dY = 0.01f;
    }

    public void Move()
    {
        Canvas.SetLeft( m_Element, X );
        Canvas.SetTop( m_Element, Y );
    }
}
```

# Running a Toucan program on another thread

When we run a Toucan program, we want to execute it on another thread to prevent the UI thread from being blocked and making the application unusable.

To do that we call either the `ToucanProgram` `RunAsync` method or if using the `ToucanVm`, the `InterpretAsync` method (`RunAsync` is just a wrapper around creating a `ToucanVm` and executing`InterpretAsync`).

```cs
    vm = new ToucanVm();
    vm.InitVm();
    vm.RegisterSystemModuleCallables();
    vm.SynchronizationContext = SynchronizationContext.Current;

    // Expose CSharp objects to the Toucan virtual machine
    vm.RegisterExternalGlobalObjects( new Dictionary < string, object >()
    {
        { "gameObject", gameObject }
    } );

    ToucanCompiler compiler = new ToucanCompiler();

    var program = compiler.Compile( new[] { Code.Text } );

    // Executes the program asynchronously in a Task, to avoid blocking the UI thread
    vm.InterpretAsync( program );
```

If we run the program as-is, we will get the unfortunate message:

```
System.InvalidOperationException: 'The calling thread cannot access this object because a different thread owns it.'
```

at this line of code in our `GameObject`.

```cs
Canvas.SetLeft( m_Element, X );
```

This is because out Toucan program is running on another thread, and attempting to access objects that were created on the main thread.

To fix this issue, we need a way to run code on the main thread!

# The `sync` keyword

The `sync` keyword is used to execute anything inside a sync block in another context. This is  done by temporarily exiting the internal Run loop to switch to a `SynchronizedContext` and execute the Run loop from there. At the end of the block, execution returns to the current thread. 

```Toucan 
module Main;

while ( true ) { 
    if (gameObject.X < 0 || gameObject.X > 300 ) {
        gameObject.dX = -gameObject.dX;
    }
    gameObject.X += gameObject.dX;

    // execute on the main thread
    sync {
        gameObject.Move();
    }
}
```

The thread where the code in the `sync` block will be executed on is determined by passing a `SynchronizationContext` to the `ToucanVm`.  The assignment should be done on the UI thread.

```cs
vm.SynchronizationContext = SynchronizationContext.Current;
```

With the `sync` block in place, the bit program can now move the circle on the Canvas.
