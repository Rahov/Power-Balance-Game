using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeMonkey.Utils;
using UnityEngine.SceneManagement;

public class demandGraph : commsScript
{
	private static demandGraph instace;

	[SerializeField] 
	private Sprite 	  	dotSprite;
	private RectTransform 	graphContainer;
	private RectTransform 	labelTemplateX;
	private RectTransform 	labelTemplateY;
	private RectTransform 	dashTemplateX;
	private RectTransform 	dashTemplateY;
	private List<GameObject> gameObjectList;
	private List<IGraphVisualObject> graphVisualObjectlist;
	private GameObject 		tooltipGameObject;

	// Cached values
	private List<int> valueList;
	private IGraphVisual graphVisual;
	private int maxVisable;
	private Func<int, string> getAxisLabelX;
	private Func<float, string> getAxisLabelY; 

	private void Awake() 
	{
		instace = this;
		graphContainer = transform.Find("graphContainer").GetComponent<RectTransform>();
		labelTemplateX = graphContainer.Find("xAxis").GetComponent<RectTransform>();
		labelTemplateY = graphContainer.Find("yAxis").GetComponent<RectTransform>();
		dashTemplateX = graphContainer.Find("xDash").GetComponent<RectTransform>();
		dashTemplateY = graphContainer.Find("yDash").GetComponent<RectTransform>();
		tooltipGameObject = graphContainer.Find("tooltip").gameObject;

		gameObjectList = new List<GameObject>();
		graphVisualObjectlist = new List<IGraphVisualObject>();


		IGraphVisual lineGraphVisual = new LineGraphVisual(graphContainer, dotSprite, Color.white, new Color(1,1,1, .5f));
		IGraphVisual barChartVisual = new BarChartVisual(graphContainer, Color.white, .8f);	

		transform.Find("barChartBtn").GetComponent<Button_UI>().ClickFunc = () => {SetGraphVisual(barChartVisual);};
		transform.Find("lineGraphBtn").GetComponent<Button_UI>().ClickFunc = () => {SetGraphVisual(lineGraphVisual);};


		// Level Selection process determined by the Scene indexer
		switch (SceneManager.GetActiveScene().buildIndex)
		{
			case(2): valueList = new List<int>() { 3,4,6,9,8 };			break; // 22 houses
			case(3): valueList = new List<int>() { 10 }; 				break; // 10 
			case(4): valueList = new List<int>() { 1,2,2,3,2,1,1 };		break; // 12 houses
			case(5): valueList = new List<int>() { 2,2,2,3,4,3,4,2,2 };	break; // 22 houses
			case(6): valueList = new List<int>() { 5,5 };		break;
			case(7): valueList = new List<int>() { 5,5 }; 		break;
			case(8): valueList = new List<int>() { 5,5 };		break;
			case(9): valueList = new List<int>() { 5,5 };		break;
		//  case(5): valueList = new List<int>() {  22,23,22,21,24,27,31,29,31,34,34,34,30,34,33,32,31,30,32 };
		//  case(6): valueList = new List<int>() {  22,23,22,21,24,27,31,29,31,30,29,31,30,32,35,34,33,32,33,32,31,28,26,24 };

			default: valueList = new List<int>() { 0 }; break;
		}
		transmitter_valueList = valueList;		// transmits the values on the graph


		ShowGraph(valueList, barChartVisual, -1, (int _i) => ""+(_i), (float _f) => Mathf.RoundToInt(_f) + "  ");
  }

	public static void ShowTooltip_static(string tooltipText, Vector2 anchoredPosition) 
	{
		instace.ShowTooltip(tooltipText, anchoredPosition);
	}
	private void ShowTooltip(string tooltipText, Vector2 anchoredPosition)
	{
		tooltipGameObject.SetActive(true);

		tooltipGameObject.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;

		Text tooltipUIText = tooltipGameObject.transform.Find("text").GetComponent<Text>();
		tooltipUIText.text = tooltipText;
		float textPaddingSize = 4f;
		Vector2 backgroundSize = new Vector2(
				tooltipUIText.preferredWidth + textPaddingSize * 2f, 
				tooltipUIText.preferredHeight+ textPaddingSize * 2f
				);

		tooltipGameObject.transform.Find("background").GetComponent<RectTransform>().sizeDelta = backgroundSize;
		tooltipGameObject.transform.SetAsLastSibling();
	}
	public static void HideTooltip_static() 
	{
		instace.HideTooltip();
	}
	private void HideTooltip()
	{
		tooltipGameObject.SetActive(false);		
	}

	private void SetGraphVisual(IGraphVisual graphVisual) 
	{
		ShowGraph(this.valueList, graphVisual, this.maxVisable, this.getAxisLabelX, this.getAxisLabelY);
	}

	private void ShowGraph(List<int> valueList, IGraphVisual graphVisual, int maxVisable = -1,  Func<int, string> getAxisLabelX = null, Func<float, string> getAxisLabelY = null) 
	{
		this.valueList = valueList;
		this.graphVisual = graphVisual;
		this.getAxisLabelX = getAxisLabelX;
		this.getAxisLabelY = getAxisLabelY; 

		if (getAxisLabelX == null) 
		{
			getAxisLabelX = delegate (int _i) { return _i.ToString(); };
		}
		this.maxVisable = maxVisable;
		
		if (getAxisLabelY == null) 
		{
			getAxisLabelY = delegate (float _f) { return Mathf.RoundToInt(_f).ToString(); };
		}
		if (maxVisable <= 0)
		{
			maxVisable = valueList.Count;
		}

		foreach(GameObject gameObject in gameObjectList)
		{
			Destroy(gameObject);
		}
		gameObjectList.Clear();

		foreach(IGraphVisualObject graphVisualObject in graphVisualObjectlist)
		{
			graphVisualObject.ClearnUp();
		}
		graphVisualObjectlist.Clear();

		float graphWidth = graphContainer.sizeDelta.x;
		float graphHeight = graphContainer.sizeDelta.y;

		float yMax = valueList[0];
		float yMin = valueList[0];

		for (int i = Mathf.Max(valueList.Count - maxVisable, 0); i < valueList.Count; i++) 
		{
			int value = valueList[i];

			if (value > yMax){yMax = value;}
			if (value < yMin){yMin = value;}
		}

		float yDifference = yMax - yMin;
		if(yDifference <= 0){yDifference = 5f;}
		yMax = yMax +((yDifference)* 0.2f);
		yMin = yMin -((yDifference)* 0.2f);

		yMin = 0; // set graph.y to zero 

		float xSize = graphWidth / (maxVisable + 1);
		int xIndex = 0;
		transmitter_graphOffset = xSize; // transmit the offset requred 
		//Debug.Log(xSize);

		for (int i = Mathf.Max(valueList.Count - maxVisable, 0); i < valueList.Count; i++) 
		{
			float xPosition = xSize + xIndex * xSize;
			float yPosition = ((valueList[i]-yMin)/(yMax - yMin)) * graphHeight;
			
			// Debug.Log(xPosition); // this is the index size 
			transmitter_graphHeight = yPosition; // last object height

			string tooltipText = getAxisLabelY(valueList[i]);
			graphVisualObjectlist.Add(graphVisual.CreateGraphVisualObject(new Vector2(xPosition, yPosition), xSize, tooltipText));

			RectTransform labelX = Instantiate(labelTemplateX);
			labelX.SetParent(graphContainer, false);
			labelX.gameObject.SetActive(true);
			labelX.anchoredPosition = new Vector2(xPosition, -7f);
			labelX.GetComponent<Text>().text = getAxisLabelX(i);
			gameObjectList.Add(labelX.gameObject);
			
			RectTransform dashX = Instantiate(dashTemplateX);
			dashX.SetParent(graphContainer, false);
			dashX.gameObject.SetActive(true);
			dashX.anchoredPosition = new Vector2(xPosition, -3f);
			gameObjectList.Add(dashX.gameObject);

			xIndex++;
		}
		transmitter_graphSize = xIndex; // transmits the number of values on the graph

		int separatorCount = 10;
		for (int i = 0; i <= separatorCount; i++) {
			RectTransform labelY = Instantiate(labelTemplateY);
			labelY.SetParent(graphContainer, false);
			labelY.gameObject.SetActive(true);
			float normalizedValue = i * 1f / separatorCount;
			labelY.anchoredPosition = new Vector2(-7f, normalizedValue * graphHeight);
			labelY.GetComponent<Text>().text = getAxisLabelY(yMin + (normalizedValue * (yMax - yMin)));
			gameObjectList.Add(labelY.gameObject);
			
			RectTransform dashY = Instantiate(dashTemplateY);
			dashY.SetParent(graphContainer, false);
			dashY.gameObject.SetActive(true);
			dashY.anchoredPosition = new Vector2(-4f, normalizedValue * graphHeight);
			gameObjectList.Add(dashY.gameObject);
		}
	}

	// showinmg visual data poings
	private interface IGraphVisual
	{
		IGraphVisualObject CreateGraphVisualObject(Vector2 graphPosition, float graphPositionWidth, string tooltipText);
	}
	
	// single visual object in the graph
	private interface IGraphVisualObject
	{
		void SetGraphVisualObjectInfo(Vector2 graphPosition, float graphPositionWidth, string tooltipText);
		void ClearnUp();

	}

	private class BarChartVisual : IGraphVisual
	{
		private RectTransform graphContainer;
		private Color barColor;
		private float barWidthMultiplier;

		public BarChartVisual(RectTransform graphContainer, Color barColor, float barWidthMultiplier)
		{
			this.graphContainer = graphContainer;
			this.barColor = barColor;
			this.barWidthMultiplier = barWidthMultiplier;
		}
		public IGraphVisualObject CreateGraphVisualObject(Vector2 graphPosition, float graphPositionWidth, string tooltipText)
		{
			GameObject barGameObject = CreateBar(graphPosition, graphPositionWidth);

			BarChartVisualObject barChartVisualObject =  new BarChartVisualObject(barGameObject, barWidthMultiplier);
			barChartVisualObject.SetGraphVisualObjectInfo(graphPosition, graphPositionWidth, tooltipText);

			return barChartVisualObject;
		}		

		private GameObject CreateBar(Vector2 graphPosition, float barWidth)
		{
			GameObject gameObject = new GameObject("bar", typeof(Image));
			gameObject.transform.SetParent(graphContainer, false);
			gameObject.GetComponent<Image>().color = barColor;
			RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
			rectTransform.anchoredPosition = new Vector2(graphPosition.x, 0f);
			rectTransform.sizeDelta = new Vector2(barWidth * barWidthMultiplier, graphPosition.y);
			rectTransform.anchorMin = new Vector2(0, 0);
			rectTransform.anchorMax = new Vector2(0, 0);
			rectTransform.pivot = new Vector2(.5f, 0f);
			
			Button_UI barButtonUI = gameObject.AddComponent<Button_UI>();

			transmitter_graphObject = gameObject; // transmits the bar object to the comms class
			
			return gameObject;
		}

		public class BarChartVisualObject : IGraphVisualObject
		{
			private GameObject barGameObject;			
			private float barWidthMultiplier;			

			public BarChartVisualObject(GameObject barGameObject, float barWidthMultiplier)
			{
				this.barGameObject = barGameObject;			
				this.barWidthMultiplier = barWidthMultiplier;
			}
			public void SetGraphVisualObjectInfo(Vector2 graphPosition, float graphPositionWidth, string tooltipText)
			{
				RectTransform rectTransform = barGameObject.GetComponent<RectTransform>();
				rectTransform.anchoredPosition = new Vector2(graphPosition.x, 0f);
				rectTransform.sizeDelta = new Vector2(graphPositionWidth * barWidthMultiplier, graphPosition.y);

				Button_UI barButtonUI = barGameObject.GetComponent<Button_UI>();

				barButtonUI.MouseOverOnceFunc = () => {
					ShowTooltip_static(tooltipText, graphPosition);
				};
				barButtonUI.MouseOutOnceFunc = () => {
					HideTooltip_static();
				};
			}
			public void ClearnUp()
			{
				Destroy(barGameObject);
			}

		}
	}
	private class LineGraphVisual : IGraphVisual 
	{
		private RectTransform graphContainer;
		private Sprite dotSprite;
		private LineGraphVisualObject lastLineGraphVisualObject;		
		private Color dotConnectionColor;
		private Color dotColor;

		public LineGraphVisual(RectTransform graphContainer, Sprite dotSprite, Color dotColor, Color dotConnectionColor)
		{
			this.graphContainer = graphContainer;
			this.dotSprite = dotSprite;
			this.dotConnectionColor = dotConnectionColor;
			this.dotColor = dotColor;
			lastLineGraphVisualObject = null;
		}
		public IGraphVisualObject CreateGraphVisualObject(Vector2 graphPosition, float graphPositionWidth, string tooltipText)
		{
			GameObject dotGameObject = CreateDot(graphPosition);



			//gameObjectList.Add(dotGameObject);
			GameObject dotConnectionGameObject = null; 
			if (lastLineGraphVisualObject != null) 
			{
				dotConnectionGameObject = CreateDotConnection(lastLineGraphVisualObject.GetGrahpPosition(), dotGameObject.GetComponent<RectTransform>().anchoredPosition);
			}

			LineGraphVisualObject lineGraphVisualObject = new LineGraphVisualObject(dotGameObject, dotConnectionGameObject, lastLineGraphVisualObject);
			lineGraphVisualObject.SetGraphVisualObjectInfo(graphPosition, graphPositionWidth, tooltipText);
			lastLineGraphVisualObject = lineGraphVisualObject;

			return lineGraphVisualObject;
		}	

		private GameObject CreateDot(Vector2 anchoredPosition) 
		{
			GameObject gameObject = new GameObject("dot", typeof(Image));
			gameObject.transform.SetParent(graphContainer, false);
			gameObject.GetComponent<Image>().sprite = dotSprite;
			gameObject.GetComponent<Image>().color = dotColor;
			RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
			rectTransform.anchoredPosition = anchoredPosition;
			rectTransform.sizeDelta = new Vector2(11, 11);
			rectTransform.anchorMin = new Vector2(0, 0);
			rectTransform.anchorMax = new Vector2(0, 0);

			Button_UI dotButtonUI = gameObject.AddComponent<Button_UI>();

			return gameObject;
		}

		private GameObject CreateDotConnection(Vector2 dotPositionA, Vector2 dotPositionB) {
			GameObject gameObject = new GameObject("dotConnection", typeof(Image));
			gameObject.transform.SetParent(graphContainer, false);
			gameObject.GetComponent<Image>().color = dotConnectionColor;			
			RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
			Vector2 dir = (dotPositionB - dotPositionA).normalized;
			float distance = Vector2.Distance(dotPositionA, dotPositionB);
			rectTransform.anchorMin = new Vector2(0, 0);
			rectTransform.anchorMax = new Vector2(0, 0);
			rectTransform.sizeDelta = new Vector2(distance, 3f);
			rectTransform.anchoredPosition = dotPositionA + dir * distance * .5f;
			rectTransform.localEulerAngles = new Vector3(0, 0, UtilsClass.GetAngleFromVectorFloat(dir));
			return gameObject;
		}

		public class LineGraphVisualObject : IGraphVisualObject
		{ 
			public event EventHandler OnGraphChange;

			private GameObject dotGameObject;
			private GameObject dotConnectionGameObject;
			private LineGraphVisualObject lastVisualObject;

			public LineGraphVisualObject(GameObject dotGameObject, GameObject dotConnectionGameObject, LineGraphVisualObject lastVisualObject)
			{
				this.dotGameObject = dotGameObject;
				this.dotConnectionGameObject = dotConnectionGameObject;
				this.lastVisualObject = lastVisualObject;

				if (lastVisualObject != null)
				{
					lastVisualObject.OnGraphChange += lastVisualObject_OnGraphChange;
				}
			}

			private void lastVisualObject_OnGraphChange(object sender, EventArgs e)
			{
				UpdateDotConnection();
			}

			public void SetGraphVisualObjectInfo(Vector2 graphPosition, float graphPositionWidth, string tooltipText)
			{
				RectTransform rectTransform = dotGameObject.GetComponent<RectTransform>();
				rectTransform.anchoredPosition = graphPosition;

				UpdateDotConnection();

				Button_UI dotButtonUI = dotGameObject.GetComponent<Button_UI>();
				dotButtonUI.MouseOverOnceFunc += () => {
					ShowTooltip_static(tooltipText, graphPosition);
				};
				dotButtonUI.MouseOutOnceFunc += () => {
					HideTooltip_static();
				};
		
				if (OnGraphChange != null) { OnGraphChange(this, EventArgs.Empty); }
			}

			public void ClearnUp()
			{
				Destroy(dotGameObject);
				Destroy(dotConnectionGameObject);
			}
			public Vector2 GetGrahpPosition()
			{
				RectTransform rectTransform = dotGameObject.GetComponent<RectTransform>();
				return rectTransform.anchoredPosition;
			}

			private void UpdateDotConnection() 
			{
				if (dotConnectionGameObject != null)
				{
					RectTransform dotConnectionRectTransform = dotConnectionGameObject.GetComponent<RectTransform>();
					Vector2 dir = (lastVisualObject.GetGrahpPosition() - GetGrahpPosition()).normalized;
					float distance = Vector2.Distance(GetGrahpPosition(), lastVisualObject.GetGrahpPosition());
					dotConnectionRectTransform.sizeDelta = new Vector2(distance, 3f);
					dotConnectionRectTransform.anchoredPosition = GetGrahpPosition() + dir * distance * .5f;
					dotConnectionRectTransform.localEulerAngles = new Vector3(0, 0, UtilsClass.GetAngleFromVectorFloat(dir));
				}		
			}
		}	
	}
}