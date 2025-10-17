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
        public IHttpActionResult Get(int id)
        {
            // Validación de Entrada (ID)
            if (id <= 0)
            {
                // Retorna 400 Bad Request
                return BadRequest("El ID de artículo debe ser un número entero positivo para la búsqueda.");
            }

            try
            {
                var negocio = new ArticuloNegocio();

               
                List<Articulo> listaDeDominio = negocio.Listar();
                Articulo articuloDominio = listaDeDominio.FirstOrDefault(a => a.Id == id);

                // Manejo de Recurso No Encontrado
                if (articuloDominio == null)
                {
                    // Retorna 404 Not Found
                    return NotFound();
                }

                // Mapeo del objeto de Dominio a DTO
                var articuloDto = new ArticuloDto
                {
                    Id = articuloDominio.Id,
                    Codigo = articuloDominio.Codigo,
                    Nombre = articuloDominio.Nombre,
                    Descripcion = articuloDominio.Descripcion,
                    Precio = articuloDominio.Precio,
                    Marca = articuloDominio.Marca?.Descripcion,
                    Categoria = articuloDominio.Categoria?.Descripcion,

                    Imagenes = articuloDominio.Imagenes?.Select(img => new ImagenDto
                    {
                        Id = img.Id,
                        UrlImagen = img.UrlImagen
                    }).ToList() ?? new List<ImagenDto>()
                };

                // Respuesta Exitosa Retorna 200 OK con el ArticuloDto
                return Ok(articuloDto);
            }
            catch (Exception ex)
            {
                // Manejo de Errores retorna 500 Internal Server Error
                return InternalServerError(ex);
            }
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


        //AGR

        // POST: api/Articulo/Imagenes
        [Route("api/Articulo/Imagenes")]
        [HttpPost]
        public HttpResponseMessage PostImagenes([FromBody] ImagenAgregarDto imagenDto)
        {
            try
            {
                //  Validar que se envíen datos
                if (imagenDto == null)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "No se enviaron datos");

                if (imagenDto.IdProducto <= 0)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Id del producto invalido");

                if (imagenDto.Imagenes == null || !imagenDto.Imagenes.Any())
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Debe enviar al menos una imagen");

                
                var articuloNegocio = new ArticuloNegocio();
                var productoExistente = articuloNegocio.Listar().FirstOrDefault(a => a.Id == imagenDto.IdProducto);
                if (productoExistente == null)
                    return Request.CreateResponse(HttpStatusCode.NotFound, "El producto no existe");

                // Validar que las URLs de imagen no estén vacías
                foreach (var url in imagenDto.Imagenes)
                {
                    if (string.IsNullOrWhiteSpace(url))
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Una o mas URLs de imagen son invalidas");
                }

                
                articuloNegocio.AgregarImagenes(imagenDto.IdProducto, imagenDto.Imagenes);

                //Respuesta exitosa
                return Request.CreateResponse(HttpStatusCode.Created, "Imagenes agregadas correctamente");
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Ocurrio un error inesperado al agregar imagenes");
            }

        }


        // PUT: api/Producto/5
        [HttpPut]
        [Route("api/Articulo/{id}")]
        public HttpResponseMessage Put(int id, [FromBody] ArticuloAltaDto articuloDto)
        {
            try
            {
                if (articuloDto == null)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "No se enviaron datos del producto");

                if (!ModelState.IsValid)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ModelState);

                if (string.IsNullOrWhiteSpace(articuloDto.Nombre))
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "El nombre del producto es obligatorio");

                if (articuloDto.Precio <= 0)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "El precio debe ser mayor a 0");

                var marcaNegocio = new MarcaNegocio();
                var categoriaNegocio = new CategoriaNegocio();

                Marca marcaExistente = marcaNegocio.listar().Find(m => m.Id == articuloDto.IdMarca);
                if (marcaExistente == null)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "La marca especificada no existe");

                Categoria categoriaExistente = categoriaNegocio.listar().Find(c => c.Id == articuloDto.IdCategoria);
                if (categoriaExistente == null)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "La categoria especificada no existe");

                var articuloNegocio = new ArticuloNegocio();
                Articulo articuloExistente = articuloNegocio.Listar().FirstOrDefault(a => a.Id == id);
                if (articuloExistente == null)
                    return Request.CreateResponse(HttpStatusCode.NotFound, "El producto no existe");

                articuloExistente.Codigo = articuloDto.Codigo;
                articuloExistente.Nombre = articuloDto.Nombre;
                articuloExistente.Descripcion = articuloDto.Descripcion;
                articuloExistente.Precio = articuloDto.Precio;
                articuloExistente.Marca = marcaExistente;
                articuloExistente.Categoria = categoriaExistente;

                articuloNegocio.modificar(articuloExistente);

                return Request.CreateResponse(HttpStatusCode.OK, "Producto modificado correctamente");
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Ocurrio un error al modificar el producto");
            }
        }



        // DELETE: api/Producto/5
        public void Delete(int id)
        {

            ArticuloNegocio negocio = new ArticuloNegocio();
            negocio.EliminarFisico(id);

        }
    }
}
