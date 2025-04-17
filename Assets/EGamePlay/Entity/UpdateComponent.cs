namespace EGamePlay
{
    public class UpdateComponent : Component
    {
        public override bool DefaultEnable { get; set; } = true;

        public override void Update()
        {
            Entity.Update();
        }
    }
}