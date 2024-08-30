using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

namespace UltrahapticsCoreAsset.Examples.Polyline
{
    public class FigureControllerEndPoint : MonoBehaviour
    {
        public string endpointUrl = ""; // Cambia esto a la URL correcta
        private SensationSource sensationSource;

        public GameObject linePrefab;

        private GameObject activeFigure; // Para mantener una referencia a la figura activa

        public float requestInterval = 2.0f; // Intervalo de tiempo entre peticiones en segundos

        void Start()
        {
            sensationSource = FindObjectOfType<SensationSource>(); // Encuentra el SensationSource en la escena

            if (sensationSource == null)
            {
                Debug.LogError("SensationSource no está asignado.");
                return;
            }

            // Asegúrate de que el SensationSource esté desactivado al inicio
            sensationSource.Running = false;

            StartCoroutine(CheckFigureStatusRepeatedly());
        }

        IEnumerator CheckFigureStatusRepeatedly()
        {
            while (true)
            {
                yield return GetFigureStatus();
                yield return new WaitForSeconds(requestInterval); // Espera antes de realizar la siguiente petición
            }
        }

        IEnumerator GetFigureStatus()
        {
            UnityWebRequest request = UnityWebRequest.Get(endpointUrl);
            request.timeout = 5; // Ajusta el tiempo de espera si es necesario

            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                Debug.LogError("Error: " + request.error);
            }
            else
            {
                Debug.Log("Response: " + request.downloadHandler.text);

                // Aquí analizas la respuesta y decides qué figura generar o detener
                ProcessResponse(request.downloadHandler.text);
            }
        }

        void ProcessResponse(string responseText)
        {
            try
            {
                // Parsear la respuesta JSON para obtener el estado, tratando como un array
                var jsonDataArray = JsonUtility.FromJson<Wrapper<ResponseData>>(WrapArray(responseText));

                if (jsonDataArray.items.Length > 0)
                {
                    // Si el estado es 1, activamos la figura (línea en este caso)
                    if (jsonDataArray.items[0].state == 1)
                    {
                        ActivateFigure("line");
                    }
                    // Si el estado es 0, detenemos cualquier figura activa
                    else if (jsonDataArray.items[0].state == 0)
                    {
                        DeactivateFigure();
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error al procesar la respuesta: " + e.Message);
            }
        }

        void ActivateFigure(string figureType)
        {
            // Desactivamos la figura actual antes de activar una nueva
            DeactivateFigure();

            if (figureType == "line")
            {
                activeFigure = Instantiate(linePrefab, Vector3.zero, Quaternion.identity);

                // Activar el SensationSource para que la figura se reproduzca
                if (sensationSource != null)
                {
                    sensationSource.SensationBlock = "Polyline6"; // Asegúrate de que este bloque existe
                    sensationSource.Running = true;
                }
            }
        }

        void DeactivateFigure()
        {
            // Si hay una figura activa, la destruimos
            if (activeFigure != null)
            {
                Destroy(activeFigure);
                activeFigure = null;

                // Desactivar el SensationSource para detener la reproducción
                if (sensationSource != null)
                {
                    sensationSource.Running = false;
                }

                Debug.Log("Figura desactivada");
            }
        }

        [System.Serializable]
        private class ResponseData
        {
            public int id;
            public string temperature;
            public int state;
        }

        [System.Serializable]
        private class Wrapper<T>
        {
            public T[] items;
        }

        private string WrapArray(string json)
        {
            return "{\"items\":" + json + "}";
        }
    }
}