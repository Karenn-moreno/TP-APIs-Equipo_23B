using dominio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace negocio
{
    public class CategoriaNegocio
    {
        public List<Categoria> listar()
        {
            List<Categoria> lista = new List<Categoria>();
            AccesoDatos datos = new AccesoDatos();

            try
            {
                datos.setearConsulta("SELECT Id, Descripcion FROM CATEGORIAS");
                datos.ejecutarLectura();

                while (datos.Lector.Read())
                {
                    Categoria aux = new Categoria();
                    aux.Id = (int)datos.Lector["Id"];
                    aux.Descripcion = (string)datos.Lector["Descripcion"];

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

        public bool AgregarSiNoExiste(Categoria nuevaCategoria)
        {
            int count = 0;

            
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT COUNT(*) AS Total FROM CATEGORIAS WHERE Descripcion=@descripcion");
                datos.setearParametro("@descripcion", nuevaCategoria.Descripcion);
                datos.ejecutarLectura();

                if (datos.Lector.Read())
                    count = (int)datos.Lector["Total"];
            }
            finally
            {
                datos.cerrarConexion();
            }

            if (count > 0)
                return false;

            
            AccesoDatos datosInsert = new AccesoDatos();
            try
            {
                datosInsert.setearConsulta("INSERT INTO CATEGORIAS (Descripcion) VALUES (@descripcion)");
                datosInsert.setearParametro("@descripcion", nuevaCategoria.Descripcion);
                int filasAfectadas = datosInsert.ejecutarAccionInt();
                return filasAfectadas > 0;
            }
            finally
            {
                datosInsert.cerrarConexion();
            }
        }

        public bool ModificarCategoria(Categoria categoria)
        {
            int count = 0;

            AccesoDatos datosCheck = new AccesoDatos();
            try
            {
                datosCheck.setearConsulta(
                    "SELECT COUNT(*) AS Total FROM CATEGORIAS WHERE Descripcion=@descripcion AND Id != @id");
                datosCheck.setearParametro("@descripcion", categoria.Descripcion);
                datosCheck.setearParametro("@id", categoria.Id);
                datosCheck.ejecutarLectura();

                if (datosCheck.Lector.Read())
                    count = (int)datosCheck.Lector["Total"];
            }
            finally
            {
                datosCheck.cerrarConexion();
            }

            if (count > 0)
                return false; 

            
            AccesoDatos datosUpdate = new AccesoDatos();
            try
            {
                datosUpdate.setearConsulta(
                    "UPDATE CATEGORIAS SET Descripcion=@descripcion WHERE Id=@id");
                datosUpdate.setearParametro("@descripcion", categoria.Descripcion);
                datosUpdate.setearParametro("@id", categoria.Id);

                int filasAfectadas = datosUpdate.ejecutarAccionInt();
                return filasAfectadas > 0;
            }
            finally
            {
                datosUpdate.cerrarConexion();
            }
        }

        public void EliminarCategoria(int id)
        {
            try
            {
                AccesoDatos datos = new AccesoDatos();
                datos.setearConsulta("delete from CATEGORIAS where id=@id");
                datos.setearParametro("@id", id);
                datos.ejecutarAccion();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }



    }
}
