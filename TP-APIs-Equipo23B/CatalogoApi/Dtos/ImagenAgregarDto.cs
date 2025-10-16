using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CatalogoApi.Dtos
{
    public class ImagenAgregarDto
    {
        public int IdProducto { get; set; }
        public List<string> Imagenes { get; set; }
    }
}