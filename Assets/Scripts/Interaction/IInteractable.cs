namespace AmishSimulator
{
    public interface IInteractable
    {
        string InteractionLabel { get; }
        float InteractionRadius { get; }
        void Interact();
    }
}
