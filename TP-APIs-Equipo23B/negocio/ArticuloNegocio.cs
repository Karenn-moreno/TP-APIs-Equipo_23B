using dominio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

//comentario 
namespace negocio
{
    public class ArticuloNegocio
    {
        private readonly ImagenNegocio imagenNegocio = new ImagenNegocio();
        public List<Articulo> Listar()
        {
            List<Articulo> lista = new List<Articulo>();
            AccesoDatos datos = new AccesoDatos();

            try
            {
                datos.abrirConexion(); 
                // obtenemos todos los articulos
                datos.setearConsulta(@"
                    SELECT a.Id, a.Codigo, a.Nombre, a.Descripcion,
                           m.Id as IdMarca, m.Descripcion AS Marca,
                           c.Id as IdCategoria, c.Descripcion AS Categoria,
                           a.Precio
                    FROM ARTICULOS a
                    INNER JOIN MARCAS m ON a.IdMarca = m.Id
                    INNER JOIN CATEGORIAS c ON a.IdCategoria = c.Id");
                datos.ejecutarLectura();

                while (datos.Lector.Read())
                {
                    Articulo articulo = new Articulo();
                    articulo.Id = (int)datos.Lector["Id"];
                    articulo.Codigo = (string)datos.Lector["Codigo"];
                    articulo.Nombre = (string)datos.Lector["Nombre"];
                    articulo.Descripcion = (string)datos.Lector["Descripcion"];
                    articulo.Precio = (decimal)datos.Lector["Precio"];

                    articulo.Marca = new Marca { Id = (int)datos.Lector["IdMarca"], Descripcion = (string)datos.Lector["Marca"] };
                    articulo.Categoria = new Categoria { Id = (int)datos.Lector["IdCategoria"], Descripcion = (string)datos.Lector["Categoria"] };

                    lista.Add(articulo);
                }
               

                // obtenemos las imagenes 
                ImagenNegocio imagenNegocio = new ImagenNegocio();
                List<Imagen> listaImagenes = imagenNegocio.ListarTodas();

                // asignamos las imagenes a cada articulo
                foreach (Articulo art in lista)
                {
                    art.Imagenes = listaImagenes.Where(img => img.IdArticulo == art.Id).ToList();
                }

                return lista;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }
        public Articulo Listar(int id)// sobrecargamos el método Listar
        {
            
            AccesoDatos datos = new AccesoDatos();
            Articulo articulo = null;
            try
            {
                datos.abrirConexion();
                datos.setearConsulta(@"
            SELECT a.Id, a.Codigo, a.Nombre, a.Descripcion,
                   m.Descripcion AS Marca,
                   c.Descripcion AS Categoria,
                   a.Precio
            FROM ARTICULOS a
            INNER JOIN MARCAS m ON a.IdMarca = m.Id
            INNER JOIN CATEGORIAS c ON a.IdCategoria = c.Id
            WHERE a.Id = @id");
                datos.setearParametro("@id", id);
                datos.ejecutarLectura();

                if (datos.Lector.Read())
                {
                    articulo = new Articulo();
                    articulo.Id = (int)datos.Lector["Id"];
                    articulo.Codigo = (string)datos.Lector["Codigo"];
                    articulo.Nombre = (string)datos.Lector["Nombre"];
                    articulo.Descripcion = (string)datos.Lector["Descripcion"];

                    articulo.Marca = new Marca();
                    articulo.Marca.Descripcion = (string)datos.Lector["Marca"];

                    articulo.Categoria = new Categoria();
                    articulo.Categoria.Descripcion = (string)datos.Lector["Categoria"];

                    articulo.Precio = (decimal)datos.Lector["Precio"];

                    ImagenNegocio imagenNegocio = new ImagenNegocio();
                    articulo.Imagenes = imagenNegocio.ListarPorArticulo(id);
                }

                return articulo;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }
        //nuevo articulo
        public void Agregar(Articulo nuevo)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.abrirConexion();

                string consultaArticulo = "INSERT INTO ARTICULOS (Codigo, Nombre, Descripcion, IdMarca, IdCategoria, Precio) " +
                                          "VALUES (@Codigo, @Nombre, @Descripcion, @IdMarca, @IdCategoria, @Precio);" +
                                          "SELECT SCOPE_IDENTITY();";

                datos.setearConsulta(consultaArticulo);
                datos.setearParametro("@Codigo", nuevo.Codigo);
                datos.setearParametro("@Nombre", nuevo.Nombre);
                datos.setearParametro("@Descripcion", nuevo.Descripcion);
                datos.setearParametro("@IdMarca", nuevo.Marca.Id);
                datos.setearParametro("@IdCategoria", nuevo.Categoria.Id);
                datos.setearParametro("@Precio", nuevo.Precio);




                int nuevoId = Convert.ToInt32(datos.ejecutarEscalar());

                if (nuevoId > 0 && nuevo.Imagenes != null && nuevo.Imagenes.Count > 0)
                {
                    foreach (var imagen in nuevo.Imagenes)
                    {
                       
                        datos.limpiarParametros();

                        
                        datos.setearConsulta("INSERT INTO IMAGENES (IdArticulo, ImagenUrl) VALUES (@IdArticulo, @ImagenUrl)");

                        
                        datos.setearParametro("@IdArticulo", nuevoId);
                        datos.setearParametro("@ImagenUrl", imagen.UrlImagen);

                        
                        datos.ejecutarAccion();
                    }
                }
            }
            catch (Exception ex)
            {
                
                throw ex;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public void modificar(Articulo art)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.abrirConexion();

                // Actualiza el artículo
                datos.setearConsulta("UPDATE ARTICULOS SET Codigo=@codigo, Nombre=@nombre, Descripcion=@descripcion, IdMarca=@idMarca, IdCategoria=@idCategoria, Precio=@precio WHERE Id=@id");
                datos.setearParametro("@codigo", art.Codigo);
                datos.setearParametro("@nombre", art.Nombre);
                datos.setearParametro("@descripcion", art.Descripcion);
                datos.setearParametro("@idMarca", art.Marca.Id);
                datos.setearParametro("@idCategoria", art.Categoria.Id);
                datos.setearParametro("@precio", art.Precio);
                datos.setearParametro("@id", art.Id);
                datos.ejecutarAccion();

                //Borra las imágenes anteriores
                datos.limpiarParametros();
                datos.setearConsulta("DELETE FROM IMAGENES WHERE IdArticulo = @idArticulo");
                datos.setearParametro("@idArticulo", art.Id);
                datos.ejecutarAccion();

                // Agrega las nuevas imágenes 
                if (art.Imagenes != null && art.Imagenes.Any())
                {
                    foreach (var imagen in art.Imagenes)
                    {
                        datos.limpiarParametros();
                        datos.setearConsulta("INSERT INTO IMAGENES (IdArticulo, ImagenUrl) VALUES (@IdArticulo, @ImagenUrl)");
                        datos.setearParametro("@IdArticulo", art.Id);
                        datos.setearParametro("@ImagenUrl", imagen.UrlImagen);
                        datos.ejecutarAccion();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public void eliminar(int id)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("DELETE FROM IMAGENES WHERE IdArticulo = @id; DELETE FROM ARTICULOS WHERE id=@id;");
                datos.setearParametro("@id", id);
                datos.ejecutarAccionAutonoma();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public bool ExisteCodigo(string codigo)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.abrirConexion();
                datos.setearConsulta("SELECT 1 FROM ARTICULOS WHERE Codigo = @cod");
                datos.setearParametro("@cod", codigo);
                datos.ejecutarLectura();
                return datos.Lector.Read();
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public List<Articulo> filtrar(string campo, string criterio, string filtro)
        {
            List<Articulo> lista = new List<Articulo>();
            AccesoDatos datos = new AccesoDatos();
            try
            {
                string consulta = @"SELECT a.Id, a.Codigo, a.Nombre, a.Descripcion, m.Descripcion AS Marca, c.Descripcion AS Categoria, a.Precio, a.IdMarca, a.IdCategoria FROM ARTICULOS a INNER JOIN MARCAS m ON a.IdMarca = m.Id INNER JOIN CATEGORIAS c ON a.IdCategoria = c.Id WHERE ";

                if (campo == "Precio")
                {
                    switch (criterio)
                    {
                        case "Mayor a": consulta += "a.Precio > @filtro"; break;
                        case "Menor a": consulta += "a.Precio < @filtro"; break;
                        default: consulta += "a.Precio = @filtro"; break;
                    }
                    datos.setearParametro("@filtro", decimal.Parse(filtro));
                }
                else
                {
                    string columna = (campo == "Nombre") ? "a.Nombre" : "a.Descripcion";
                    switch (criterio)
                    {
                        case "Empieza con": consulta += columna + " LIKE @filtro + '%'"; break;
                        case "Termina con": consulta += columna + " LIKE '%' + @filtro"; break;
                        default: consulta += columna + " LIKE '%' + @filtro + '%'"; break;
                    }
                    datos.setearParametro("@filtro", filtro);
                }

                datos.abrirConexion();
                datos.setearConsulta(consulta);
                datos.ejecutarLectura();

                while (datos.Lector.Read())
                {
                    Articulo articulo = new Articulo();
                    articulo.Id = (int)datos.Lector["Id"];
                    articulo.Codigo = (string)datos.Lector["Codigo"];
                    articulo.Nombre = (string)datos.Lector["Nombre"];
                    articulo.Descripcion = (string)datos.Lector["Descripcion"];
                    articulo.Precio = (decimal)datos.Lector["Precio"];
                    articulo.Marca = new Marca { Id = (int)datos.Lector["IdMarca"], Descripcion = (string)datos.Lector["Marca"] };
                    articulo.Categoria = new Categoria { Id = (int)datos.Lector["IdCategoria"], Descripcion = (string)datos.Lector["Categoria"] };
                    lista.Add(articulo);
                }
                return lista;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public bool EliminarFisico(int id)
        {
            // 
            if (id <= 0)
            {
                throw new ArgumentException("El ID del artículo debe ser un número positivo.");
            }

            AccesoDatos datos = new AccesoDatos();
            int filasArticuloAfectadas = 0;

            try
            {
                // DELEGAR la ELIMINACIÓN de IMÁGENES (Clave Foránea)
               
                imagenNegocio.EliminarPorArticulo(id);

                
                // ELIMINAR ARTÍCULO
               
                datos.setearConsulta("DELETE FROM ARTICULOS WHERE Id = @idArticulo");
                datos.setearParametro("@idArticulo", id);

                // Ejecuta el DELETE. Retorna 1 si existe, 0 si no.
                filasArticuloAfectadas = datos.ejecutarAccionAutonoma();

                // Retorna TRUE si se eliminó una o más filas.
                return filasArticuloAfectadas > 0;
            }
            catch (Exception ex)
            {
                // Relanzamos la excepción para proporcionar contexto al error de DB.
                throw new Exception("Error al realizar la eliminación física del artículo en la base de datos.", ex);
            }
        }

    }
}
