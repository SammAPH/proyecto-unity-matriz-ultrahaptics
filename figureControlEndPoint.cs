using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

namespace UltrahapticsCoreAsset.Examples.Polyline
{
    public class FigureControllerEndPoint : MonoBehaviour
    {
        public string endpointUrl = ""; // Cambia esto a la URL correcta
        private SensationSource sensationSource;
        public LineRenderer lineRenderer;

        public RectTransform Point0;
        public RectTransform Point1;
        public RectTransform Point2;
        public RectTransform Point3;
        public RectTransform Point4;
        public RectTransform Point5;

        private float scaling = 3000.0f;

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
                var jsonDataArray = JsonUtility.FromJson<Wrapper<ResponseData>>(WrapArray(responseText));

                if (jsonDataArray.items.Length > 0)
                {
                    // Según el estado, activamos la figura correspondiente
                    switch (jsonDataArray.items[0].figures)
                    {
                        case 1:
                            ActivateFigure("line");
                            break;
                        case 2:
                            ActivateFigure("polygon");
                            break;
                        case 3:
                            ActivateFigure("triangle");
                            break;
                        case 4:
                            ActivateFigure("square");
                            break;
                        default:
                            DeactivateFigure();
                            break;
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
            DeactivateFigure(); // Desactiva cualquier figura activa

            if (figureType == "line")
            {
                SetLine();
            }
            else if (figureType == "polygon")
            {
                SetPentagon();
            }
            else if (figureType == "triangle")
            {
                SetTriangle();
            }
            else if (figureType == "square")
            {
                SetSquare();
            }

            if (sensationSource != null)
            {
                sensationSource.SensationBlock = "Polyline6"; // Asegúrate de que este bloque existe
                sensationSource.Running = true;
            }
        }

        void DeactivateFigure()
        {
            if (sensationSource != null)
            {
                sensationSource.Running = false;
            }

            Debug.Log("Figura desactivada");
        }

        void SetLine()
        {
            Point0.localPosition = new Vector3(-0.04f, 0.0f, 0.0f);
            Point1.localPosition = new Vector3(0.04f, 0.0f, 0.0f);
            Point2.localPosition = new Vector3(0.04f, 0.0f, 0.0f);
            Point3.localPosition = new Vector3(0.04f, 0.0f, 0.0f);
            Point4.localPosition = new Vector3(0.04f, 0.0f, 0.0f);
            Point5.localPosition = new Vector3(0.04f, 0.0f, 0.0f);
            Debug.Log("Figura: linea");
            ScalePoints();
            UpdateSensationInputs();
        }

        void SetTriangle()
        {
            Point0.localPosition = new Vector3(0.0f, 0.045f, 0.0f);
            Point1.localPosition = new Vector3(0.025f, -0.04f, 0.0f);
            Point2.localPosition = new Vector3(-0.025f, -0.04f, 0.0f);
            Point3.localPosition = new Vector3(0.0f, 0.045f, 0.0f);
            Point4.localPosition = new Vector3(0.0f, 0.045f, 0.0f);
            Point5.localPosition = new Vector3(0.0f, 0.045f, 0.0f);
            Debug.Log("Figura: triangulo");

            ScalePoints();
            UpdateSensationInputs();
        }

        void SetSquare()
        {
            Point0.localPosition = new Vector3(-0.025f, 0.025f, 0.0f);
            Point1.localPosition = new Vector3(0.025f, 0.025f, 0.0f);
            Point2.localPosition = new Vector3(0.025f, -0.025f, 0.0f);
            Point3.localPosition = new Vector3(-0.025f, -0.025f, 0.0f);
            Point4.localPosition = new Vector3(-0.025f, 0.025f, 0.0f);
            Point5.localPosition = new Vector3(-0.025f, 0.025f, 0.0f);
            Debug.Log("Figura: Cuadrado");

            ScalePoints();
            UpdateSensationInputs();
        }

        void SetPentagon()
        {
            Point0.localPosition = new Vector3(0.0f, 0.025f, 0.0f);
            Point1.localPosition = new Vector3(0.024f, 0.008f, 0.0f);
            Point2.localPosition = new Vector3(0.015f, -0.02f, 0.0f);
            Point3.localPosition = new Vector3(-0.015f, -0.02f, 0.0f);
            Point4.localPosition = new Vector3(-0.024f, 0.008f, 0.0f);
            Point5.localPosition = new Vector3(0.0f, 0.025f, 0.0f);
            Debug.Log("Figura: pentagono");

            ScalePoints();
            UpdateSensationInputs();
        }

        void ScalePoints()
        {
            Point0.localPosition *= scaling;
            Point1.localPosition *= scaling;
            Point2.localPosition *= scaling;
            Point3.localPosition *= scaling;
            Point4.localPosition *= scaling;
            Point5.localPosition *= scaling;
        }

        void UpdateSensationInputs()
        {
            // Asignamos directamente los valores si SensationSource está disponible
            if (sensationSource != null)
            {
                sensationSource.Inputs["point0"].Value = Point0.localPosition / scaling;
                sensationSource.Inputs["point1"].Value = Point1.localPosition / scaling;
                sensationSource.Inputs["point2"].Value = Point2.localPosition / scaling;
                sensationSource.Inputs["point3"].Value = Point3.localPosition / scaling;
                sensationSource.Inputs["point4"].Value = Point4.localPosition / scaling;
                sensationSource.Inputs["point5"].Value = Point5.localPosition / scaling;
            }
        }

        [System.Serializable]
        private class ResponseData
        {
            public int id;
            public string temperature;
            public int figures;
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