using System.ComponentModel.DataAnnotations;

namespace MyUser.API.Models.Dto
{
    public class ProductDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "enter a name value")]
        [MaxLength(50)]
        public string Name { get; set; }
        [MaxLength(100)]
        public string Description { get; set; }
        public decimal Price { get; set; }

    }
}
