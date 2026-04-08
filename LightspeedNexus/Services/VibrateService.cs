using Lightspeed.Services;

namespace LightspeedNexus.Services;

/// <summary>
/// The shared libaray doesn't have access to the vibration API, so this is just a stub implementation that does nothing.
/// Vibration is only used in the mobile apps.
/// </summary>
public class VibrateService : IVibrateService
{
    public void Start() { }
    public void Stop() { }
}
