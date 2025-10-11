using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using dominio;
using negocio;
using CatalogoApi.Dtos;

namespace CatalogoApi.Controllers
{
    public class ArticuloController : ApiController
    {
        // GET: api/Producto
        public IEnumerable<ArticuloDto> Get()
        {
            ArticuloNegocio negocio = new ArticuloNegocio();
            List<Articulo> listaDeDominio = negocio.Listar();

            // se mapea la lista de dominio a la lista de DTOs
            var listaDto = listaDeDominio.Select(art => new ArticuloDto
            {
                Id = art.Id,
                Codigo = art.Codigo,
                Nombre = art.Nombre,
                Descripcion = art.Descripcion,
                Precio = art.Precio,
                Marca = art.Marca.Descripcion,
                Categoria = art.Categoria.Descripcion,
                Imagenes = art.Imagenes.Select(img => new ImagenDto
                {
                    Id = img.Id,
                    UrlImagen = img.UrlImagen
                }).ToList()
            }).ToList();

            //se devuelve la lista de DTOs directamente
            return listaDto;
        }
        

        // GET: api/Producto/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Producto
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Producto/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Producto/5
        public void Delete(int id)
        {
        }
    }
}
