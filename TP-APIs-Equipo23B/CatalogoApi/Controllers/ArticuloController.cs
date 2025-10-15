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
        [HttpPost]
        public HttpResponseMessage Post([FromBody] ArticuloAltaDto articuloDto)
        {
            // validar el modelo primero 
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);

            // instanciar las capas de negocio 
            var articuloNegocio = new ArticuloNegocio();
            var marcaNegocio = new MarcaNegocio();
            var categoriaNegocio = new CategoriaNegocio();

            // validar que la Marca y la Categoría enviadas existan en la BD
            Marca marcaExistente = marcaNegocio.listar().Find(m => m.Id == articuloDto.IdMarca);
            Categoria categoriaExistente = categoriaNegocio.listar().Find(c => c.Id == articuloDto.IdCategoria);

            // si alguna no existe, devolver un error 
            if (marcaExistente == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "La marca especificada no existe");
            }

            if (categoriaExistente == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "La categoria especificada no existe");
            }

            //mapear la nueva entidad Articulo
            var nuevoArticulo = new Articulo
            {
                Codigo = articuloDto.Codigo,
                Nombre = articuloDto.Nombre,
                Descripcion = articuloDto.Descripcion,
                Precio = articuloDto.Precio,
                Marca = marcaExistente,         
                Categoria = categoriaExistente, 
                Imagenes = articuloDto.Imagenes?
                    .Select(imgDto => new Imagen { UrlImagen = imgDto.UrlImagen })
                    .ToList() ?? new List<Imagen>()
            };

            // llamar al método para agregar el nuevo artículo
            articuloNegocio.Agregar(nuevoArticulo);

            //devolver con mensaje de éxito
            return Request.CreateResponse(HttpStatusCode.OK, "Articulo agregado correctamente!");
        }
    

        // PUT: api/Producto/5
        public void Put(int id, [FromBody] ArticuloAltaDto articuloDto)
        {
            ArticuloNegocio negocio = new ArticuloNegocio();
            Articulo nuevo = new Articulo();

            nuevo.Id = id;
            nuevo.Codigo = articuloDto.Codigo;
            nuevo.Nombre = articuloDto.Nombre;
            nuevo.Descripcion = articuloDto.Descripcion;
            nuevo.Precio = articuloDto.Precio;
            nuevo.Marca = new Marca { Id = articuloDto.IdMarca };
            nuevo.Categoria = new Categoria { Id = articuloDto.IdCategoria };

            negocio.modificar(nuevo);

        }

        // DELETE: api/Producto/5
        public void Delete(int id)
        {
        }
    }
}
