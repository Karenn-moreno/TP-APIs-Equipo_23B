using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CatalogoApi.Dtos
{
    public class ArticuloDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }

    
        public string Marca { get; set; }
        public string Categoria { get; set; }

        // lista del dto de Imagen, no de la entidad de dominio
        public List<ImagenDto> Imagenes { get; set; }
    }
}