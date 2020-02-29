using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class commsScript : MonoBehaviour 
{	
	#region Variables
	/*-------Private Variables------- 
	* 	all private variables have the "m_" notation
	*
	*	1) all m_barSize(s) contain the size of a individual energy type
	*	2) m_old_investAmount is a variable used for manipulation of the m_yPosition
	*	3) m_yPostion contains the heigh at which objects are to be placed (starts at 30)
	*	4) m_graphColor contains the color of the bar to be created (optional)
	*/

	// could figure how to free up memory/space for these above
	private Color m_graphColor;
	private float m_old_investAmount = 0f;
	private float[] m_yPostion = new float[24] {30f,30f,30f,30f,30f,30f,30f,30f,30f,30f,30f,30,30f,30f,30f,30f,30f,30f,30f,30f,30f,30f,30f,30f};


	/*-------Public Variables------- 
	* 	all public variables do not have special notations
	*
	*	1) barSizeList contains the size of each energy type
	*	2) barInvestList is a table containing the investment size per location on the graph
	*	3) total_investAmount contains the total investment for each bar object on the graph
	*	4) total_barSize contains the size of the bar object obtained from demandGraph.cs
	*	5) energyunit is a ratio used for scaleing bar objects in demandGraph.cs
	*/
	public float[,] barSizeList = new float[24,5];
	public float[,] barInvestList = new float[24, 5];
	public float[] total_investAmount = new float[24] {0f,0f,0f,0f,0f,0f,0f,0f,0f,0f,0f,0f,0f,0f,0f,0f,0f,0f,0f,0f,0f,0f,0f,0f};
	public static float[] total_barSize = new float[24] ;		


	public float energyUnit;	// to be reworked such that the value is obtained from the demandGraph


	/*-------Communication functions-------
	*	Protected members accesable to child classes 
	* 
	*	1) all "c_" notations are obtained from other child classes
	*	2) all 2transmitter_" notations are used within child classes
	*/
	// obtains the size of the list containing all values
	public static int c_size;
    protected static int transmitter_graphSize
    {
        get {return c_size;}
		set {c_size = value;}
	}

	// obtain the requred visual height of energy units from demandGraph
	public static float c_graphHeight;
	protected static  float transmitter_graphHeight
	{
        get {return c_graphHeight;}
		set {c_graphHeight = value;}
	} 

	// obtains the list of values produced in demandGraph
	public static List<int> c_valueList;
    protected static List<int> transmitter_valueList
    {
        get {return c_valueList;}
		set {c_valueList = value;}
	}

	// obtains a copy of a bar-game object from demandGraph
	private static GameObject c_graphObject;
	protected static  GameObject transmitter_graphObject
	{
        get {return c_graphObject;}
		set {c_graphObject = value;}
	} 
	
	private static float c_graphOffset;
	protected static  float transmitter_graphOffset
	{
        get {return c_graphOffset;}
		set {c_graphOffset = value;}
	} 
	#endregion

	#region Functions

	/*---------getLimits Information---------
    *	Function call at the start of supplyScript.cs
	*	Creates limits for purchasing depending on the
	*	size of the graph.
    */
	public void getLimits()
	{
		energyUnit = c_graphHeight/c_valueList[c_size-1];

		for(int j = 0; j < c_size; j++)
		{
			total_barSize[j] = c_valueList[j]*energyUnit;
			Debug.Log("index: "+j+" BarSize: "+total_barSize[j]);
		}
		for(int i = 0; i < c_size; i++) // c_size is the amount of hours we have on the graph
		{
			barSizeList[i,0] = energyUnit*5; 	// Solar
			barSizeList[i,1] = energyUnit*3f; 	// Coal
			barSizeList[i,2] = energyUnit*5f;	// Wind
			barSizeList[i,3] = energyUnit*15f;	// CCGT
			barSizeList[i,4] = energyUnit*8f; 	// Nuke
		}
	}

    /*---------getBarObject Information---------
    *	Function call for creating a different type of bar objects 
    *	Inputs are typically handled by the Supply Script
    *	
    *	1) _investAmount is the loop controller, determined by the Units input field in Supply.cs
    *	2) _graphIndex determines the location of the bar objects
    *	3) _button determines the type of energy we're using
    *	4) _buttonIndex is used for managing the capacity of each energy type
    */
	public void getBarObject(float _investAmount, int _graphIndex, string _button, int _buttonIndex)
	{	
		//getLimits(); // get the limits of the current level 

		switch (_button)
		{
			case "solarPurchaseBtn":m_graphColor = Color.green; break;
			case "coalPurchaseBtn": m_graphColor = Color.red;   break;
			case "windPurchaseBtn": m_graphColor = Color.green; break;
			case "gasPurchaseBtn":  m_graphColor = Color.red;	break;
			case "nukePurchaseBtn": m_graphColor = Color.green; break;
			default:
				Debug.Log("invalid input!");
				break;
		}

		for (int i = 0; i < _investAmount; i++)
		{	
			//barInvestList[_graphIndex, _buttonIndex] += energyUnit;
			total_investAmount[_graphIndex] += energyUnit;

			if ((total_barSize[_graphIndex] - total_investAmount[_graphIndex]) >= 0)
			{
				m_old_investAmount = energyUnit;
				makeBar(_graphIndex);
			}
			else if ((total_barSize[_graphIndex] - total_investAmount[_graphIndex]) < 0)
			{
				m_graphColor = Color.gray; // overdraft mechanic for future reference 
				makeBar(_graphIndex);
			}
		}
		m_old_investAmount = 0;
	}

	public void updateInvestSize(int _buttonIndex, float _investAmount, int _graphIndex)
	{
		barInvestList[_graphIndex, _buttonIndex] += energyUnit*_investAmount;
	}

	/*---------makeBar Information---------
	*	Creates an instance of a bar object from the demandGraph.cs 
	*	and manipulates it depending on the input from supplyScript.cs
	*
	*	1) _barlocation is defined by the passed value from supplyScript.cs
	*/
	private void makeBar(int _barlocationIndex)
	{
		GameObject barCopyObject = Instantiate(c_graphObject);	
		RectTransform _rectTransform = barCopyObject.GetComponent<RectTransform>();

		barCopyObject.transform.position = new Vector2(40f + c_graphOffset + c_graphOffset * (_barlocationIndex), m_yPostion[_barlocationIndex]);
		m_yPostion[_barlocationIndex] = m_yPostion[_barlocationIndex] + m_old_investAmount;
		_rectTransform.sizeDelta = new Vector2(_rectTransform.sizeDelta.x, energyUnit); // unable to manipulate a single vector at a time (for size)
		barCopyObject.GetComponent<Image>().color = m_graphColor;

		barCopyObject.transform.parent = transform;
	}
	#endregion //end of Function region
}

// 	old experiments
/*

		// WORKS
		m_total_barSize = c_valueList[m_barIndex]*m_energyUnit;
		for (int i = 0; i < _investAmount; i++)
		{	
			m_total_investAmount += m_energyUnit;
			m_InvestList[m_localindex] += m_energyUnit;

			if ((m_total_barSize - m_total_investAmount) > 0)
			{
				makeBar();

				m_old_investAmount = m_energyUnit;
			} 
			else if ((m_total_barSize - m_total_investAmount) < 0)
			{
				m_barIndex++;
				m_offset = 20f;
				m_yPostion = 30f;
				m_old_investAmount = 0f; 
				m_InvestList[m_localindex] = 0f;
				m_total_investAmount = 0;
				
				makeBar();
			}


		m_Type = _investType;
		if (m_Type != m_old_Type)
		{
			m_old_Type = m_Type;
			m_barIndex = 0 + m_old_barIndex;
		}

		m_total_barSize = c_valueList[m_barIndex]*m_energyUnit; // store the total bar height 

		for (int i = 0; i < _investAmount; i++)
		{	
			m_text_investments[m_barIndex] += m_energyUnit;
			m_InvestList[m_localindex] += m_energyUnit;

			if ((m_barSizeList[m_localindex] - m_InvestList[m_localindex]) > 0)
			{
				makeBar();

				m_old_investAmount = m_energyUnit;
				Debug.Log("-----Total investments: " + m_text_investments[m_barIndex] + ", at index " + m_barIndex);
				Debug.Log("-----Total barSize: " + m_total_barSize + ", at index " + m_barIndex);
			}
			else if ((m_barSizeList[m_localindex] - m_InvestList[m_localindex]) < 0)
			{
				Debug.Log("------------we jumed an index------------");
				m_barIndex++;
				m_offset = 20f;
				m_yPostion = 30f;
				m_old_investAmount = 0f; 
				m_text_investments[m_barIndex] = 0;
				m_InvestList[m_localindex] = 0f; // might have to move lower eventually

				
				makeBar();
			}



	public void getObject(float investAmount)
	{
		m_investAmount = investAmount*3f;	// increase the visual size of the invested amount
		m_totalAmount += m_investAmount;	// store all the investments made for a single bar
		m_indexSize  = c_valueList[m_localindex]*14.2857f - m_b_ySize;	// used for manipulation of the bar size (14.2857f is the scale size for 24 bars)

		if ((m_totalAmount - m_indexSize) < 0) // compares the size of the bar against the total investments(*3) 
		{
			GameObject barCopyObject = Instantiate(c_graphObject);	// create a gameobject copy of a visual bar game object from (demandCurve.cs)
			RectTransform _rectTransform = barCopyObject.GetComponent<RectTransform>(); // store the rect transfrom of the game object
			m_yPostion = m_yPostion + m_old_investAmount + m_a_ySize;	// add the previous investment amount to the y position of the object
			barCopyObject.transform.position = new Vector2(60f + m_offset * m_localindex, m_yPostion); 	// goes through all the bars on the graph
			_rectTransform.sizeDelta = new Vector2(16, m_investAmount);	// scale the size of the object by the investment amount (16 is the width of the bars)
			barCopyObject.GetComponent<Image>().color = Color.green;	// make the game object of colour green


			barCopyObject.transform.parent = transform;	// magic line of code that made instantiation visable in game scene
			m_old_investAmount = m_investAmount;		// store the previous investment
			m_a_ySize = 0; 
		}

		else if ((m_totalAmount - m_indexSize) > 0)	// if we've surpassed the size of the bar -> shift to the next bar
		{
			m_a_ySize = m_investAmount - (m_totalAmount - m_indexSize);
			m_b_ySize = m_old_investAmount - (m_investAmount - (m_totalAmount - m_indexSize));		

			if (m_a_ySize < 0)
			{
				m_a_ySize = m_b_ySize*-1;
			}

			GameObject a_barCopy = Instantiate(c_graphObject);	
			RectTransform a_rectTransform = a_barCopy.GetComponent<RectTransform>();
			m_yPostion = m_yPostion + m_old_investAmount;	
			a_barCopy.transform.position = new Vector2(60f + m_offset * m_localindex, m_yPostion); 	
			a_rectTransform.sizeDelta = new Vector2(16, m_a_ySize);	
			a_barCopy.GetComponent<Image>().color = Color.green;
			a_barCopy.transform.parent = transform;

			m_localindex++;
			m_offset = 20f;
			m_yPostion = 30f; 

			if (m_b_ySize < 0)
			{
				m_b_ySize = m_b_ySize*-1;
			}
			
			if (m_localindex > 23)
			{
				return;
			}
			GameObject b_barCopy = Instantiate(c_graphObject);	
			RectTransform b_rectTransform = b_barCopy.GetComponent<RectTransform>();
			b_barCopy.transform.position = new Vector2(60f + m_offset * m_localindex, m_yPostion); 	
			b_rectTransform.sizeDelta = new Vector2(16, m_b_ySize);	
			b_barCopy.GetComponent<Image>().color = Color.green;
			b_barCopy.transform.parent = transform;

			m_old_investAmount = 0;
			m_totalAmount = 0f;
			m_a_ySize = m_b_ySize;
		}
 */