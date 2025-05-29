# External State Machine System for PopStateMachine

This document explains how to use the external state machine system that has been added to the PopStateMachine.

## Overview

The external state machine system allows you to attach custom behavior controllers to Pops without modifying the core PopStateMachine code. External state machines can:

- Override the default tick behavior
- Intercept and modify state change requests
- Have different priorities when multiple are attached
- Be enabled/disabled at runtime

## Key Components

### 1. IExternalStateMachine Interface

The core interface that external state machines must implement:

```csharp
public interface IExternalStateMachine
{
    void OnAttached(PopStateMachine popStateMachine, Pop pop);
    void OnDetached();
    bool Tick();
    IState OnStateChangeRequested(IState requestedState);
    bool IsActive { get; }
    int Priority { get; set; }
}
```

### 2. ExternalStateMachineBase Class

A base class that provides common functionality for external state machines. Inherit from this class to avoid implementing all interface methods:

```csharp
public abstract class ExternalStateMachineBase : IExternalStateMachine
{
    // Provides default implementations and helper methods
    protected void ChangeState(IState newState);
    protected void ForceChangeState(IState newState);
    protected void TickDefault();
    protected IState CurrentState { get; }
}
```

### 3. ExternalStateMachineManager Component

A Unity MonoBehaviour that makes it easy to attach external state machines through the Inspector:

- Automatically finds the PopStateMachine on the GameObject
- Manages attachment/detachment of external state machines
- Provides Inspector interface for configuration

## How to Create a Custom External State Machine

### Method 1: Inherit from ExternalStateMachineBase (Recommended)

```csharp
public class MyCustomStateMachine : ExternalStateMachineBase
{
    public override bool Tick()
    {
        if (!IsActive)
            return false;

        // Custom logic here
        if (SomeCondition())
        {
            ChangeState(new MyCustomState());
            return true; // We handled the tick
        }

        // Use default behavior
        TickDefault();
        return true;
    }

    public override IState OnStateChangeRequested(IState requestedState)
    {
        // Intercept and modify state changes
        if (requestedState is ForageState)
        {
            return new IdleState(); // Replace foraging with idle
        }
        
        return requestedState; // Allow the change
    }
}
```

### Method 2: Implement IExternalStateMachine Directly

```csharp
public class MyCustomStateMachine : MonoBehaviour, IExternalStateMachine
{
    private PopStateMachine popStateMachine;
    private Pop pop;
    
    public bool IsActive { get; private set; } = true;
    public int Priority { get; set; } = 0;

    public void OnAttached(PopStateMachine psm, Pop p)
    {
        popStateMachine = psm;
        pop = p;
    }

    public void OnDetached()
    {
        popStateMachine = null;
        pop = null;
    }

    public bool Tick()
    {
        // Your custom logic here
        return true; // Return true if you handled the tick
    }

    public IState OnStateChangeRequested(IState requestedState)
    {
        // Return the state to actually use, or null to prevent the change
        return requestedState;
    }
}
```

## How to Use External State Machines

### Option 1: Use ExternalStateMachineManager (Easiest)

1. Add the `ExternalStateMachineManager` component to your Pop GameObject
2. Add your external state machine components to the same GameObject
3. Drag the external state machine components into the "External State Machine Components" list in the manager
4. The manager will automatically attach them when the game starts

### Option 2: Attach Programmatically

```csharp
// Get the PopStateMachine
PopStateMachine popStateMachine = pop.GetComponent<PopStateMachine>();

// Create and attach external state machine
MyCustomStateMachine customSM = pop.gameObject.AddComponent<MyCustomStateMachine>();
customSM.Priority = 10; // Higher priority
popStateMachine.AttachExternalStateMachine(customSM);

// Detach when no longer needed
popStateMachine.DetachExternalStateMachine(customSM);
```

### Option 3: Direct Control

```csharp
PopStateMachine popStateMachine = pop.GetComponent<PopStateMachine>();

// Disable external state machines temporarily
popStateMachine.AllowExternalStateMachines = false;

// Force a state change that bypasses external state machines
popStateMachine.ForceChangeState(new IdleState());

// Use default tick behavior directly
popStateMachine.TickDefault();
```

## Priority System

When multiple external state machines are attached:

- Higher priority numbers are checked first
- The first active external state machine that returns `true` from `Tick()` stops further processing
- For state changes, the first active external state machine that returns a non-null state wins

## Best Practices

1. **Always check IsActive**: Make sure your external state machine respects the `IsActive` property
2. **Use Priority wisely**: Set appropriate priorities to ensure the correct order of execution
3. **Handle null cases**: Always check for null references in your external state machines
4. **Call TickDefault() when appropriate**: Use this to maintain default behavior alongside your custom logic
5. **Clean up properly**: Implement `OnDetachedInternal()` to clean up any resources

## Example: Making Pops More Social

```csharp
public class SocialStateMachine : ExternalStateMachineBase
{
    [SerializeField] private float socialRadius = 5f;
    [SerializeField] private float socialChance = 0.1f;

    public override bool Tick()
    {
        if (!IsActive)
            return false;

        // Look for nearby Pops
        Collider[] nearbyPops = Physics.OverlapSphere(pop.transform.position, socialRadius);
        
        if (nearbyPops.Length > 1 && Random.value < socialChance)
        {
            // Find nearest Pop and move towards them
            float nearestDistance = float.MaxValue;
            Transform nearestPop = null;
            
            foreach (var collider in nearbyPops)
            {
                if (collider.transform != pop.transform)
                {
                    float distance = Vector3.Distance(pop.transform.position, collider.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestPop = collider.transform;
                    }
                }
            }
            
            if (nearestPop != null)
            {
                // Create a custom social state or use existing movement
                ChangeState(new WanderState()); // Could be enhanced to move toward the target
                return true;
            }
        }

        // Use default behavior
        TickDefault();
        return true;
    }
}
```

## Backward Compatibility

The system is fully backward compatible:

- Existing Pops will continue to work exactly as before
- No external state machines attached = original behavior
- The `AllowExternalStateMachines` flag can disable the system entirely
- `ForceChangeState()` and `TickDefault()` provide access to original functionality
