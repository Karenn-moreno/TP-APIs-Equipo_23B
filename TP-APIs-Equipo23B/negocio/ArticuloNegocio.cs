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
        public List<Articulo> Listar()
        {
            List<Articulo> lista = new List<Articulo>();
            AccesoDatos datos = new AccesoDatos();

            try
            {
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
                datos.setearConsulta("INSERT INTO ARTICULOS (Codigo, Nombre, Descripcion, Precio, IdMarca, IdCategoria) " +
                 "VALUES ('" + nuevo.Codigo + "', '" + nuevo.Nombre + "', '" + nuevo.Descripcion + "', " + nuevo.Precio +
                ", @IdMarca, @IdCategoria)" + "INSERT INTO IMAGENES (IdArticulo, ImagenUrl) VALUES (SCOPE_IDENTITY(), @ImagenUrl);");

                datos.setearParametro("@IdMarca", nuevo.Marca.Id);
                datos.setearParametro("@IdCategoria", nuevo.Categoria.Id);

                //datos.setearParametro("@ImagenUrl", nuevo.Imagenes);
                //datos.ejecutarAccion();

                // Obtener la URL de la primera imagen
                string urlImagen = (nuevo.Imagenes != null && nuevo.Imagenes.Count > 0)
                                   ? nuevo.Imagenes[0].UrlImagen
                                   : "https://redthread.uoregon.edu/files/original/affd16fd5264cab9197da4cd1a996f820e601ee4.png";

                datos.setearParametro("@ImagenUrl", urlImagen);
                datos.ejecutarAccion();
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
                
                datos.setearConsulta("UPDATE ARTICULOS set Codigo = @codigo, Nombre = @nombre, Descripcion = @descripcion, IdMarca = @idMarca, IdCategoria = @idCategoria, Precio = @precio where Id = @id");
                datos.setearParametro("@codigo", art.Codigo);
                datos.setearParametro("@nombre", art.Nombre);
                datos.setearParametro("@descripcion", art.Descripcion);
                datos.setearParametro("@idMarca", art.Marca.Id);
                datos.setearParametro("@idCategoria", art.Categoria.Id);
                datos.setearParametro("@precio", art.Precio);
                datos.setearParametro("@id", art.Id);
                datos.ejecutarAccion();

                // Borra la imagen anterior del artículo
                datos.cerrarConexion();
                datos = new AccesoDatos();
                datos.setearConsulta("DELETE from IMAGENES where IdArticulo = @idArticulo");
                datos.setearParametro("@idArticulo", art.Id);
                datos.ejecutarAccion();

                // Agrega la nueva imagen si la hay.
                if (art.Imagenes.Count > 0)
                {
                    datos.cerrarConexion();
                    datos = new AccesoDatos();
                    datos.setearConsulta("INSERT into IMAGENES (IdArticulo, ImagenUrl) VALUES (@IdArticulo, @ImagenUrl)");
                    datos.setearParametro("@IdArticulo", art.Id);
                    datos.setearParametro("@ImagenUrl", art.Imagenes[0].UrlImagen);
                    datos.ejecutarAccion();
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
            try
            {
                AccesoDatos datos = new AccesoDatos();
                datos.setearConsulta("delete from ARTICULOS where id=@id");
                datos.setearParametro("@id", id);
                datos.ejecutarAccion();
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
                string consulta = @"SELECT a.Id, a.Codigo, a.Nombre, a.Descripcion,
                                   m.Descripcion AS Marca,
                                   c.Descripcion AS Categoria,
                                   a.Precio
                            FROM ARTICULOS a
                            INNER JOIN MARCAS m ON a.IdMarca = m.Id
                            INNER JOIN CATEGORIAS c ON a.IdCategoria = c.Id
                            WHERE ";

                if (campo == "Precio")
                {
                    switch (criterio)
                    {
                        case "Mayor a":
                            consulta += "a.Precio > " + filtro;
                            break;
                        case "Menor a":
                            consulta += "a.Precio < " + filtro;
                            break;
                        default:
                            consulta += "a.Precio = " + filtro;
                            break;
                    }
                }
                else if (campo == "Nombre")
                {
                    switch (criterio)
                    {
                        case "Empieza con":
                            consulta += "a.Nombre like '" + filtro + "%'";
                            break;
                        case "Termina con":
                            consulta += "a.Nombre like '%" + filtro + "'";
                            break;
                        default:
                            consulta += "a.Nombre like '%" + filtro + "%'";
                            break;
                    }
                }
                else if (campo == "Descripcion")
                {
                    switch (criterio)
                    {
                        case "Empieza con":
                            consulta += "a.Descripcion like '" + filtro + "%'";
                            break;
                        case "Termina con":
                            consulta += "a.Descripcion like '%" + filtro + "'";
                            break;
                        default:
                            consulta += "a.Descripcion like '%" + filtro + "%'";
                            break;
                    }
                }

                datos.setearConsulta(consulta);
                datos.ejecutarLectura();

                while (datos.Lector.Read())
                {
                    Articulo articulo = new Articulo();
                    articulo.Id = (int)datos.Lector["Id"];
                    articulo.Codigo = (string)datos.Lector["Codigo"];
                    articulo.Nombre = (string)datos.Lector["Nombre"];
                    articulo.Descripcion = (string)datos.Lector["Descripcion"];

                    articulo.Marca = new Marca();
                    articulo.Marca.Descripcion = (string)datos.Lector["Marca"];

                    articulo.Categoria = new Categoria();
                    articulo.Categoria.Descripcion = (string)datos.Lector["Categoria"];

                    articulo.Precio = (decimal)datos.Lector["Precio"];

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



    }
}
