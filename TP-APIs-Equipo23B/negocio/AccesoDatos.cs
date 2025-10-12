using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;



namespace negocio

{

    public class AccesoDatos

    {

        private readonly SqlConnection conexion;

        private SqlCommand comando;

        private SqlDataReader lector;



        public SqlDataReader Lector

        {
            get { return lector; }

        }



        public AccesoDatos()

        {

            conexion = new SqlConnection("Server=.\\SQLEXPRESS; Initial Catalog=CATALOGO_P3_DB; Integrated Security=true;");

            comando = new SqlCommand();

            comando.Connection = conexion;

        }



        public void setearConsulta(string consulta)

        {
            comando.CommandType = System.Data.CommandType.Text;

            comando.CommandText = consulta;
        }



        public void setearParametro(string nombre, object valor)

        {
            comando.Parameters.AddWithValue(nombre, valor);
        }



        public void limpiarParametros()

        {
            comando.Parameters.Clear();
        }



        public void ejecutarLectura()

        {
            try

            {
                lector = comando.ExecuteReader();
            }

            catch (Exception ex)

            {
                throw ex;
            }

        }



        public void ejecutarAccion()

        {

            try

            {

                comando.ExecuteNonQuery();

            }

            catch (Exception ex)

            {

                throw ex;

            }

        }
        public int ejecutarAccionAutonoma()
        {
            try
            {

                if (conexion.State == System.Data.ConnectionState.Closed)
                    conexion.Open();

                return comando.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
               
                if (conexion.State == System.Data.ConnectionState.Open)
                    conexion.Close();
            }
        }


        public object ejecutarEscalar()

        {

            try

            {
                return comando.ExecuteScalar();
            }

            catch (Exception ex)

            {

                throw ex;

            }

        }


        public void abrirConexion()

        {

            if (conexion.State == System.Data.ConnectionState.Closed)

            {
                conexion.Open();

            }

        }


        public void cerrarConexion()

        {
            if (lector != null)

                lector.Close();

            if (conexion.State == System.Data.ConnectionState.Open)

                conexion.Close();

        }

    }

}
