using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CatalogoApi.Dtos
{
    public class ArticuloAltaDto
    {
        [Required(ErrorMessage = "El código es obligatorio.")]
        public string Codigo { get; set; }
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; }
        public string Descripcion { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El precio no puede ser negativo!")]
        public decimal Precio { get; set; }


        [Required(ErrorMessage = "La marca es obligatoria..")]
        public int IdMarca { get; set; }
        [Required(ErrorMessage = "La categoría es obligatoria..")]
        public int IdCategoria { get; set; }


        public List<ImagenDto> Imagenes { get; set; }
    }
}