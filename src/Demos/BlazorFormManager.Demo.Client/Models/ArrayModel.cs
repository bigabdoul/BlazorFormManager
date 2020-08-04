namespace BlazorFormManager.Demo.Client.Models
{
    public class ArrayModel
    {
        public string Name { get; set; }
        public int[] Items { get; set; }
        public static int[] SampleItems => new[] { 1, 2, 3, 4, 5, 100, -20 };
    }
}
