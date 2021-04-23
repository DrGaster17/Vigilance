namespace Vigilance.EventSystem
{
    public abstract class Event
    {
        public abstract void Execute(IEventHandler handler);
    }
}
