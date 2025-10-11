using dominio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace negocio
{
    public class MarcaNegocio
    {
        public List<Marca> listar()
        {
            List<Marca> lista = new List<Marca>();
            AccesoDatos datos = new AccesoDatos();

            try
            {
                datos.setearConsulta("SELECT Id, Descripcion FROM MARCAS");
                datos.ejecutarLectura();

                while (datos.Lector.Read())
                {
                    Marca aux = new Marca();
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

        public bool InsertarSiNoExiste(Marca nuevaMarca)
        {
            int count = 0;

            // verificacion 
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT COUNT(*) AS Total FROM MARCAS WHERE Descripcion=@descripcion");
                datos.setearParametro("@descripcion", nuevaMarca.Descripcion);
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

            // insertar- nuevo objeto
            AccesoDatos datosInsert = new AccesoDatos();
            try
            {
                datosInsert.setearConsulta("INSERT INTO MARCAS (Descripcion) VALUES (@descripcion)");
                datosInsert.setearParametro("@descripcion", nuevaMarca.Descripcion);
                int filasAfectadas = datosInsert.ejecutarAccionInt();
                return filasAfectadas > 0;
            }
            finally
            {
                datosInsert.cerrarConexion();
            }
        }

        public void Eliminar(int id)
        {
            try
            {
                AccesoDatos datos = new AccesoDatos();
                datos.setearConsulta("delete from MARCAS where id=@id");
                datos.setearParametro("@id", id);
                datos.ejecutarAccion();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public bool Modificar(Marca marca)
        {
            int count = 0;

            // compruebo si ya existe una marca con el mismo nombre
            AccesoDatos datosCheck = new AccesoDatos();
            try
            {
                datosCheck.setearConsulta(
                    "SELECT COUNT(*) AS Total FROM MARCAS WHERE Descripcion=@descripcion AND Id != @id");
                datosCheck.setearParametro("@descripcion", marca.Descripcion);
                datosCheck.setearParametro("@id", marca.Id);
                datosCheck.ejecutarLectura();

                if (datosCheck.Lector.Read())
                    count = (int)datosCheck.Lector["Total"];
            }
            finally
            {
                datosCheck.cerrarConexion();
            }

            if (count > 0)
                return false; // Ya existe otra marca con esa descripción

            // actualizar la marca
            AccesoDatos datosUpdate = new AccesoDatos();
            try
            {
                datosUpdate.setearConsulta(
                    "UPDATE MARCAS SET Descripcion=@descripcion WHERE Id=@id");
                datosUpdate.setearParametro("@descripcion", marca.Descripcion);
                datosUpdate.setearParametro("@id", marca.Id);

                int filasAfectadas = datosUpdate.ejecutarAccionInt();
                return filasAfectadas > 0; // true si se modifico
            }
            finally
            {
                datosUpdate.cerrarConexion();
            }
        }



    }
}