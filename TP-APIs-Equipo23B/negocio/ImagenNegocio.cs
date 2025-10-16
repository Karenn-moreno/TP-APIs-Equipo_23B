using dominio;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace negocio
{
    public class ImagenNegocio
    {
        public List<Imagen> ListarTodas()
        {
            List<Imagen> lista = new List<Imagen>();
            AccesoDatos datos = new AccesoDatos();

            try
            {
                //  lee de la tabla Imagenes
                datos.abrirConexion();
                datos.setearConsulta("SELECT Id, IdArticulo, ImagenUrl FROM IMAGENES");
                datos.ejecutarLectura();

                while (datos.Lector.Read())
                {
                    Imagen aux = new Imagen();
                    aux.Id = (int)datos.Lector["Id"];
                    aux.IdArticulo = (int)datos.Lector["IdArticulo"];
                    if (datos.Lector.IsDBNull(datos.Lector.GetOrdinal("ImagenUrl")))
                    {
                        aux.UrlImagen = null;
                    }
                    else
                    {
                        aux.UrlImagen = (string)datos.Lector["ImagenUrl"];
                    }

                    lista.Add(aux);
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


        public List<Imagen> ListarPorArticulo(int idArticulo)
        {
            List<Imagen> lista = new List<Imagen>();
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.abrirConexion();
                datos.setearConsulta("SELECT Id, IdArticulo, ImagenUrl FROM IMAGENES WHERE IdArticulo = @idArticulo");
                datos.setearParametro("@idArticulo", idArticulo);
                datos.ejecutarLectura();

                while (datos.Lector.Read())
                {
                    Imagen img = new Imagen();
                    img.Id = (int)datos.Lector["Id"];
                    img.UrlImagen = (string)datos.Lector["ImagenUrl"];
                    lista.Add(img);
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

        public void EliminarPorArticulo(int idArticulo)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                // Sentencia SQL para borrar todas las imágenes de ese artículo
                datos.setearConsulta("DELETE FROM IMAGENES WHERE IdArticulo = @id");
                datos.setearParametro("@id", idArticulo);
                datos.ejecutarAccionAutonoma();
            }
            catch (Exception ex)
            {
                // Relanzamos la excepción con un mensaje específico para la capa
                throw new Exception("Error al eliminar las imágenes del artículo en la base de datos.", ex);
            }
        }
    }
}

