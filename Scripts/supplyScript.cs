using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeMonkey.Utils;
using Random=UnityEngine.Random;	// unity wanted this, otherwise random function gave error
using TMPro; // include textmesh pro asset
using UnityEngine.SceneManagement;

public class supplyScript : commsScript
{
	#region Variables
    /*-------Private Variables------- 
    *	all private variables have the "m_" notation
    *	all Lists are used for ease of access 
    *	
	*	1) all m_Button(s) contain the size name of the button to be found within the game scene
    *	2) m_priceList contains the price of each energy type
    *	3) m_capacityList contains the capacity of each energy type
    *	4) m_buttonList contains the names of the buttons within an array
	*	5) m_barLimists is a placeholder variable, ensuring the player doesn't purchase more than the available energy 
    *	6) m_Balance contains total amount the player has to spend
    *	7) m_counter contains the number of loops for the timer function
    *	8) m_initCounter is used as a delay for the timer function 
    *	9) m_Timer is the time frame between each loop
    *	10)m_NextTime is used for comparing the number of loops that have happened (can be used for displaying the time)
    *	11)m_multiplier is used in an input field for increasing the total amount of energy being purchased
    *	12)m_HoursIndex is used in an input field for changing between different index on the graph
    *	13)m_supplyContainer is used for identifying the graph container object within the game scene
    *	14)m_headerImage is used for  identifying the header image of the error text box
	*	15)m_totalCapital is used for identifying and manipulating m_Balance
    *	16)m_priceTag is used for identifying and manipulating the price of each energy type
    *	17)m_capacityTag is used for identifying and manipulating the capacity of each energy type
    *	18)m_unitsInputField is used for identifying and manipulating the input field for Units 
    *	19)m_indexInputField is used for identifying and manipulating the input field for the graph index
    */
	private static string m_solarButton = "solarPurchaseBtn"; // static must be used for the list to work
	private static string m_coalButton  = "coalPurchaseBtn";
	private static string m_windButton  = "windPurchaseBtn";
	private static string m_ccgtButton  = "gasPurchaseBtn";
	private static string m_nukeButton  = "nukePurchaseBtn";
	// Arrays
	private float[]  m_priceList 	= new float[5] { solarPrice, coalPrice, windPrice, ccgtPrice, nukePrice };
	private int[]  m_capacityList = new int[5] { solarCapacity, coalCapacity, windCapacity, ccgtCapacity, nukeCapacity };
	private string[] m_buttonList  	= new string[5]{ m_solarButton, m_coalButton, m_windButton, m_ccgtButton, m_nukeButton }; 
	private float[,] m_barLimits 	= new float[24,5];
	// SupplyScript
	private static float m_playerBalance = 10000;
	private static byte m_multiplier = 1;
	private static byte m_HoursIndex;
	private int m_tutorialIndex = 0;
	// Timer Function
	private static int m_counter = 20;
	private static int m_initCounter = m_counter - 1;
	private static float  m_NextTime = 0f; 
	private static float  m_Timer = 2.5f;
	// Objects (Unity Specific)
	private RectTransform m_supplyContainer;
	private RectTransform m_headerImage;
	private RectTransform m_debugContainer;
	private RectTransform m_scoreContainer;
	private RectTransform m_scoreBtn;
	private TextMeshProUGUI m_debugHeaderText;
	private TextMeshProUGUI m_debugContentText;
	private GameObject m_totalCapital;
	private GameObject m_priceTag;
	private GameObject m_capacityTag;
	private InputField m_unitsInputField;
	private InputField m_indexInputField;
	
	/*-------Private Variables------- 
    *	all public variables do not have special notations
	*	all public variables are accessable through the Unity editor	
    *	
	*	1) all Price(s) is used to contain the price of an individual energy type
    *	2) all Capacity(s) is used to contain the capacity of an individual energy type
    */
	public static float nukePrice = 0;	// mid cost should be around 95f 
	public static float solarPrice= 0;	// mid cost should be around 63f 	
	public static float windPrice = 0;	// mid cost should be around 61f 
	public static float ccgtPrice = 0;	// mid cost should be around 189f 
	public static float coalPrice = 0;	// mid cost should be around 136f 
	public static int solarCapacity= 10;       
	public static int coalCapacity = 5; 
	public static int windCapacity = 15;  
	public static int ccgtCapacity = 30;
	public static int nukeCapacity = 20;

	//===Placeholders===
	private int[] highEnergy_limit = {76,158,74,198,123}; 
	private int[] lowEnergy_limit =  {55,125,46,182,58 };
	private int completeBars = 0;
	private int m_carbonScore = 0;
	private int m_greenScore = 0;
	private int totalScore = 0;
	private int CO2_weight = 250;

	private Sprite crokSprite;
	private Sprite bisonSprite;
	private Sprite buffaloSprite;
	private Sprite carSprite;
	private Sprite sealSprite;
	private Sprite rhinoSprite;
	private Sprite pollutionSprite;



	private string euivKG;

	//==================
	#endregion


	#region Functions
    /*-------Awake Information------- 
    *	Awake is a Unity-specific function used for initialising earlier
    *	compared to other functions
    *	    
    *	The function automatically initiates on the start of the game 
    *	and primarily focuses on UI elements.
    *	Each game object expected to be manipulated (buttons) is
    *	identified within the game scene, using "Find" and "GetComponent".
    *	ClickFuncs are used to handle any on mouse click events
    */
	private void Awake()
	{
		// obtaining infromation on game objects within the scene 
		m_supplyContainer = transform.Find("supplyContainer").GetComponent<RectTransform>();	
		m_unitsInputField = m_supplyContainer.Find("unitsField").GetComponent<InputField>();
		m_indexInputField = m_supplyContainer.Find("indexField").GetComponent<InputField>();
		m_totalCapital = m_supplyContainer.Find("totalCapital").gameObject;


		// Load resources from the project file (e.g. images)
		// Files must be located in the resources folder
		carSprite = Resources.Load<Sprite>("car");
		crokSprite = Resources.Load<Sprite>("crok");
		sealSprite = Resources.Load<Sprite>("seal");
		bisonSprite = Resources.Load<Sprite>("bison");
		rhinoSprite = Resources.Load<Sprite>("rhino");
		buffaloSprite = Resources.Load<Sprite>("buff");
		pollutionSprite = Resources.Load<Sprite>("pollution");
		//=======

		//  Debug Window Objects
		m_debugContainer = 	transform.Find("debugContainer").GetComponent<RectTransform>();
		m_headerImage = m_debugContainer.Find("headerImage").GetComponent<RectTransform>();
		m_debugHeaderText = m_headerImage.Find("headerText").GetComponent<TextMeshProUGUI>();
		m_debugContentText = m_debugContainer.Find("contentText").GetComponent<TextMeshProUGUI>();
		m_unitsInputField.text = "1";

		// adding and subtracting from the inputfields
		m_unitsInputField.gameObject.transform.Find("addBtn").GetComponent<Button_UI>().ClickFunc = () => { m_multiplier++; m_unitsInputField.text = m_multiplier.ToString(); };
		m_unitsInputField.gameObject.transform.Find("subBtn").GetComponent<Button_UI>().ClickFunc = () => { m_multiplier--; m_unitsInputField.text = m_multiplier.ToString(); };
		m_indexInputField.gameObject.transform.Find("addBtn").GetComponent<Button_UI>().ClickFunc = () => { m_HoursIndex++; m_indexInputField.text = m_HoursIndex.ToString(); };
		m_indexInputField.gameObject.transform.Find("subBtn").GetComponent<Button_UI>().ClickFunc = () => { m_HoursIndex--; m_indexInputField.text = m_HoursIndex.ToString(); };

		// Manipulating the scores button 
		m_scoreBtn = transform.Find("scoresBtn").GetComponent<RectTransform>();
		m_scoreContainer = m_scoreBtn.transform.Find("scoreContainer").GetComponent<RectTransform>();
		transform.Find("scoresBtn").GetComponent<Button_UI>().ClickFunc = () => 
		{ 
			if (m_scoreContainer.gameObject.activeSelf == true)
				m_scoreContainer.gameObject.SetActive(false);
			else 
				m_scoreContainer.gameObject.SetActive(true);
		};

		/* update the capcity for the selected level
		for (int i = 0; i < 5; i++)
		{
			updateCapacity(i, m_buttonList[i]);
		}*/

		// initialisation of the game game scene in regards to the tutorial
		switch (SceneManager.GetActiveScene().buildIndex)
		{
			case(2): tutorialSettings(); break;
			case(3): level_1_Settings(); break;
			case(4): level_2_Settings(); break;
			case(5): level_3_Settings(); break;

			default: 
				Debug.Log("We're no longer in settings range!"); // disable any pup-up windows 
			break;
		}
		
		m_debugContainer.gameObject.transform.Find("nextBtn").GetComponent<Button_UI>().ClickFunc = () => 
		{
			if (m_tutorialIndex<6) {m_tutorialIndex = m_tutorialIndex+1;}
			switch (SceneManager.GetActiveScene().buildIndex)
			{
				case(2): tutorialSettings(); break;
				case(3): level_1_Settings(); break;
				case(4): level_2_Settings(); break;
				case(5): level_3_Settings(); break;
				default: Debug.Log("duck "); break;
			}
		};
		m_debugContainer.gameObject.transform.Find("backBtn").GetComponent<Button_UI>().ClickFunc = () => 
		{
			if (m_tutorialIndex>1) {m_tutorialIndex = m_tutorialIndex-1;}
			switch (SceneManager.GetActiveScene().buildIndex)
			{
				case(2): tutorialSettings(); break;
				case(3): level_1_Settings(); break;
				case(4): level_2_Settings(); break;
				default: Debug.Log("duck "); break;
			}	
		};


		// Button configuration for Purchasing energy supply 
		transform.Find(m_solarButton).GetComponent<Button_UI>().ClickFunc = () => { updateCapital(0, m_priceList[0], m_buttonList[0]); };
		transform.Find(m_coalButton ).GetComponent<Button_UI>().ClickFunc = () => { updateCapital(1, m_priceList[1], m_buttonList[1]); };	
		transform.Find(m_windButton ).GetComponent<Button_UI>().ClickFunc = () => { updateCapital(2, m_priceList[2], m_buttonList[2]); };
		transform.Find(m_ccgtButton ).GetComponent<Button_UI>().ClickFunc = () => { updateCapital(3, m_priceList[3], m_buttonList[3]); };
		transform.Find(m_nukeButton ).GetComponent<Button_UI>().ClickFunc = () => { updateCapital(4, m_priceList[4], m_buttonList[4]); };
	}

    /*-------UpdateCapital Information------- 
    *	Update capital is used for error/exception handling,
    *	as well as the purchase of energy within the game.
    *	The function is called every time on a button_UI ClickEvent.
    *	All "_" notations are considered to be local, remaining only
    *	within the function or are passed.
    *	
	*	1) _index is an index used for identifying the button
    *	2) _purchaseAmount is the amount the player is spending for energy
    *	3) _purchaseType is the string of the button used within the Unity editor
    */
	private void updateCapital(int _index, float _purchaseAmount, string _purchaseType)
	{
		updateInvestSize(_index, m_multiplier, m_HoursIndex);	


		if (_index == 0 && (m_HoursIndex < 6 || m_HoursIndex > 18))
		{
			messageWindow(true, "You're attempting to purchase solar at time: " + m_HoursIndex +". It is not possible to supply solar energy at that time.", "Apologies");		
		}
		else if ((total_barSize[m_HoursIndex] - total_investAmount[m_HoursIndex]) < 0)
		{
			messageWindow(true, "You're attempting to purchase energy beyond the expected demand for this hours.\n\nAddtionally, you've already made a reservation for energy to be supplied.", "Apologies");
		}
		else if ((barSizeList[m_HoursIndex, _index] - barInvestList[m_HoursIndex, _index]) < 0)
		{
			messageWindow(true, "You're attempting to purchase energy beyond our expected output for that hour.", "Apologies");
			barInvestList[m_HoursIndex, _index] = m_barLimits[m_HoursIndex, _index];
		}
		else if (_purchaseAmount <= m_playerBalance)
		{
			m_playerBalance -= _purchaseAmount*m_multiplier;

			updateCapacity(_index, _purchaseType);
			getBarObject(m_multiplier, m_HoursIndex, _purchaseType, _index);
			updateGameScore(_index);
			messageWindow(false, "", "");
			m_barLimits[m_HoursIndex, _index] = barInvestList[m_HoursIndex, _index];

			float remainingBar;
			remainingBar = total_barSize[m_HoursIndex] - (total_investAmount[m_HoursIndex]);
			remainingBar = Mathf.RoundToInt(remainingBar);

			if (remainingBar == 0)
			{
				completeBars = completeBars+1;
				Debug.Log(completeBars);
				Debug.Log(c_size);
				
				// check if the player has filled all the bars (won the game)
				if (completeBars == c_size)
				{
					messageWindow(true, "You've beaten the level with a score of   "+ totalScore.ToString()+"."+ euivKG, "Congradulations");
					RectTransform _winButton = m_debugContainer.transform.Find("winBtn").GetComponent<RectTransform>();
					RectTransform _winImage =  m_debugContainer.transform.Find("winImage").GetComponent<RectTransform>();
					_winButton.gameObject.SetActive(true);
					_winImage.gameObject.SetActive(true);
					transform.Find("fog").gameObject.SetActive(true);

					m_debugContainer.transform.Find("winBtn").GetComponent<Button_UI>().ClickFunc = () => 
					{
						SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
						completeBars = 0;
					};
				}
			}
		}
	}

	/*-------updateCapacity Information------- 
    *	The function is called only on a legal purchise.
    *	Update capacity is used for handling the changes in available capacity.
	*	All "_" notations are considered to be local, remaining only
	*	within the fucntion or are passed.
	*
	*	1) _index is an index used for identified the button
    *	2) _button is the string of the button used within the Unity editor
    */
	private void updateCapacity(int _index, string _button)
	{			
		RectTransform findBtn = transform.Find(_button).GetComponent<RectTransform>();
		m_capacityTag = findBtn.Find("capacity").gameObject;
		TextMeshProUGUI _capacity_textMesh = m_capacityTag.transform.Find("text").GetComponent<TextMeshProUGUI>();

		m_capacityList[_index] = m_capacityList[_index] - m_multiplier;	
		_capacity_textMesh.text = "Energy Blocks: " + m_capacityList[_index].ToString();
	}

    /*-------updatePrice Information------- 
    *	The function is called only within the timer function.
    *	All "_" notations are considered to be local, remaining only
    *	within the function or are passed.
    *	Updates the text for displaying the price of each button:
    *	Increase in price  -> red text
    *	Decrease in price  -> green text
    *	No change in price -> white text
    *	
	*	1) _value is the current price of the energy type
    *	2) _button used to distinguish each button
    *	3) _oldValue used for comparing changes in the price
    */
	private void updatePrice(float _value, float _oldValue, string _button)
	{
		RectTransform findBtn = transform.Find(_button).GetComponent<RectTransform>();
		m_priceTag = findBtn.Find("price").gameObject;
		TextMeshProUGUI price_textMesh = m_priceTag.transform.Find("text").GetComponent<TextMeshProUGUI>();
		price_textMesh.text = "Price: " + _value.ToString() + " £";

		if 		(_value > _oldValue) { price_textMesh.color = Color.green; }
		else if (_value < _oldValue) { price_textMesh.color = Color.red;   }
		else if (_value== _oldValue) { price_textMesh.color = Color.white; }
	}

	// error window control method
	private void messageWindow(bool eventTriggered, string _errorMessage, string _errorType)
	{
		if(eventTriggered)
		{
			m_debugContainer.gameObject.SetActive(true);

			/* Disable buttons if we're only considering an error

			*/
			m_debugHeaderText.text  = _errorType;
			m_debugContentText.text = _errorMessage;
		}
		else
		{
			m_debugContainer.gameObject.SetActive(false);
		}
	}

	private void tutorialSettings()
	{ 
		transform.Find("fog").gameObject.SetActive(true);

		switch (m_tutorialIndex)
		{
			case(1): messageWindow(true, "The Demand Window is on the bottom left.\n\nIt highlights objectives and citeria for completing a level. Eco-friendly energy you deliver is coloured in green, while polluting energy is red", "Game window 1"); 
			break;
			case(2): messageWindow(true, "The Action Window is on the bottom right.\n\nIt is used for interacting with the game using various buttons. You can deliver energy based on the hour it is required at, as well as increase the amount you purchase with a single click using the 'units' tab.", "Game window 2"); 
			break;
			case(3): messageWindow(true, "You'll be able to make purchases of energy blocks to satisfy the demand window. \n\nBe careful, you'll be scored depending on what energy you pick!", "Game windows 3"); 
			break;
			case(4): messageWindow(true, "Thank you for reading the tutorial, please consider playing around with the controls in this level and make sure to review your score at the end.", "End of tutorial"); 		
			break;
			case(5): Debug.Log("4");
				messageWindow(false, "", "");
				RectTransform _nextBtn = m_debugContainer.Find("nextBtn").GetComponent<RectTransform>();
				RectTransform _backtBtn = m_debugContainer.Find("backBtn").GetComponent<RectTransform>();
				_nextBtn.gameObject.SetActive(false);
				_backtBtn.gameObject.SetActive(false); 		
				transform.Find("fog").gameObject.SetActive(false);
			break;			
			default: Debug.Log("index is at 0 or beyond the scope");

			break;
		}
	}

	private void level_1_Settings() // to be developed - introduce progression, scoring
	{
		//===initialise level settings===
		transform.Find(m_solarButton).gameObject.SetActive(false);
		transform.Find(m_windButton ).gameObject.SetActive(false);
		transform.Find(m_coalButton ).gameObject.SetActive(false);
		m_supplyContainer.Find("totalCapital").gameObject.SetActive(false);
		transform.Find("fog").gameObject.SetActive(true);

		// player settings

		m_playerBalance = 2500;		
		switch (m_tutorialIndex)
		{
			case(1): 			
				messageWindow(true, "You're asked to complete a single hour of demand of 10 energy blocks. \n\nYou must complete the task with a positive score!", "Tasks"); 
			break;
			case(2): 
				messageWindow(true, "You have access only to nuclear and gas energy. \n\nGood Luck.", "Tools"); 
			break;
			case(3): 
				messageWindow(false, "", "");
				RectTransform _nextBtn = m_debugContainer.Find("nextBtn").GetComponent<RectTransform>();
				RectTransform _backtBtn = m_debugContainer.Find("backBtn").GetComponent<RectTransform>();
				_nextBtn.gameObject.SetActive(false);
				_backtBtn.gameObject.SetActive(false); 		
				transform.Find("fog").gameObject.SetActive(false);
			break;			
			default: Debug.Log("index is at 0 or beyond the scope");

			break;
		}	

	}
	private void level_2_Settings() 
	{
		//===initialise level settings===
		transform.Find(m_solarButton).gameObject.SetActive(false);
		transform.Find(m_windButton ).gameObject.SetActive(false);
		transform.Find(m_coalButton ).gameObject.SetActive(false);
		transform.Find("fog").gameObject.SetActive(true);

		// player settings

		m_playerBalance = 5000;		
		switch (m_tutorialIndex)
		{
			case(1): 			
				messageWindow(true, "You're tasked with completing another single hour of 12 energy demand blocks. \n\nTry to complete the task with a positive score and having enough money to spare!", "Tasks"); 
			break;
			case(2):
				messageWindow(true, "You have access only to nuclear and gas energy. However, this time you will need to keep a close eye on your capital, make sure you don't run out. \n\nGood Luck.", "Tools"); 
			break;
			case(3):
				messageWindow(false, "", "");
				RectTransform _nextBtn = m_debugContainer.Find("nextBtn").GetComponent<RectTransform>();
				RectTransform _backtBtn = m_debugContainer.Find("backBtn").GetComponent<RectTransform>();
				_nextBtn.gameObject.SetActive(false);
				_backtBtn.gameObject.SetActive(false); 		
				transform.Find("fog").gameObject.SetActive(false);
			break;			
			default: Debug.Log("index is at 0 or beyond the scope");

			break;
		}
	}
	
	private void level_3_Settings() 
	{
		//===initialise level settings===
		transform.Find(m_coalButton ).gameObject.SetActive(false);
		transform.Find("fog").gameObject.SetActive(true);

		// player settings

		m_playerBalance = 5000;		
		switch (m_tutorialIndex)
		{
			case(1): 			
				messageWindow(true, "You will notice that in this level, you have gotten more energy demand blocks. Don't panic, there is an upside to it as well.", "Tasks"); 
			break;
			case(2):
				messageWindow(true, "In this level, you gain access to the wind & solar energy types. These sources are renewable and will give you a positive score, so make sure you utilise them well. \n\nGood Luck.", "Tools"); 
			break;
			case(3):
				messageWindow(false, "", "");
				RectTransform _nextBtn = m_debugContainer.Find("nextBtn").GetComponent<RectTransform>();
				RectTransform _backtBtn = m_debugContainer.Find("backBtn").GetComponent<RectTransform>();
				_nextBtn.gameObject.SetActive(false);
				_backtBtn.gameObject.SetActive(false); 		
				transform.Find("fog").gameObject.SetActive(false);
			break;			
			default: Debug.Log("index is at 0 or beyond the scope");

			break;
		}
	}
	
	private void updateGameScore(int _purchaseType) // to be researched 1-coal(x3) 3-ccgt(x2)
	{
		TextMeshProUGUI _carbonScore = m_scoreContainer.Find("carbon").GetComponent<TextMeshProUGUI>();
		TextMeshProUGUI _greenScore = m_scoreContainer.Find("green").GetComponent<TextMeshProUGUI>();
		TextMeshProUGUI _total = m_scoreContainer.Find("total").GetComponent<TextMeshProUGUI>();
		Image _winSprite =  m_debugContainer.transform.Find("winImage").GetComponent<Image>();
		
		switch (_purchaseType)
		{
			case(1): _purchaseType = 320; // coal
			m_carbonScore += _purchaseType;
			_carbonScore.text = " Carbon Gen:      \t" + m_carbonScore.ToString() + " kg";
			break;
			case(3): _purchaseType = 200; // gas
			m_carbonScore += _purchaseType;
			_carbonScore.text = " Carbon Gen:      \t" + m_carbonScore.ToString() + " kg";
			break;
			default: _purchaseType = 220; // green
			m_greenScore += _purchaseType;
			_greenScore.text = " Green Gen:      \t" + m_greenScore.ToString() + " kg";
			break;		
		}
		totalScore = m_greenScore - m_carbonScore;
		_total.text = " Total:        \t" +totalScore.ToString();

		if (totalScore < 0){euivKG = " Unfortunetly, you've supplied pollution, insead of providing eco-friendly energy."; _winSprite.sprite = pollutionSprite;}
		else if (totalScore < 250){euivKG =  " That's the equivelent of a Crocodile's weight."; _winSprite.sprite = crokSprite;}
		else if (totalScore < 500){euivKG =  " That's the equivelent of a Water buffalo's weight."; _winSprite.sprite = buffaloSprite;} // insert image object activate commands here
		else if (totalScore < 1000){euivKG = " That's the equivelent of a American Bison's weight."; _winSprite.sprite = bisonSprite;}
		else if (totalScore < 1500){euivKG = " That's the equivelent of a medium sized car's weight."; _winSprite.sprite = carSprite;}
		else if (totalScore < 2000){euivKG = " That's the equivelent of a northern elephant seal's weight."; _winSprite.sprite = sealSprite;}
		else if (totalScore < 2500){euivKG = " That's the equivelent of a white rhinoceros' weight."; _winSprite.sprite = rhinoSprite;}
	}



	/*-------Update Information------- 
	*	Update is a Unity-specific function which is called every 3 frames.
	*	The fucntion is used with a timer function to delay updates in the price 
	*	to every 5 seconds.
	*	The function is used to update the player's total capital 
	*	and inputfield(s) every 3 frames.
    */
	void Update()
	{
		TextMeshProUGUI text_mesh = m_totalCapital.transform.Find("text").GetComponent<TextMeshProUGUI>();
		text_mesh.text = "Total Capital: " + m_playerBalance.ToString() + "£";

		byte.TryParse(m_unitsInputField.text, out m_multiplier);
		if (m_unitsInputField.text == "0") // Check if we're buying with negative
		{
			m_multiplier = 1;
			m_unitsInputField.text = "1";	
			messageWindow(true, "It is not possible to make a purchase with negative units.", "illigal action");
		}

		byte.TryParse(m_indexInputField.text, out m_HoursIndex);
		if(m_HoursIndex > c_size-1) // because hours start at 0, c_size must be 1 less than the actual size
		{
			m_HoursIndex = 0;
			m_indexInputField.text = "0";
			messageWindow(true, "It is not possible to attempt a purchase outside the displayed hours!", "illigal action");
		}

		// call functions we need only once 
		if (m_counter > m_initCounter && Time.fixedTime > m_NextTime)
		{

			m_NextTime = Time.fixedTime + m_Timer;
			for (int i = 0; i < m_buttonList.Length; i++)
			{
				m_priceList[i] = Random.Range(lowEnergy_limit[i],highEnergy_limit[i]);
				updatePrice(m_priceList[i], m_priceList[i], m_buttonList[i]);
			}
			m_counter--;
		}
		else if (m_counter > 0 && Time.fixedTime > m_NextTime)
		{
			getLimits();

			m_NextTime = Time.fixedTime + m_Timer;
			// Debug.Log("Timer: " + m_NextTime); // prints time to the console
			
			for(int j = 0; j < m_buttonList.Length; j++)
			{
				float prevousPrice = m_priceList[j];
				m_priceList[j] = Random.Range(lowEnergy_limit[j],highEnergy_limit[j]);
				updatePrice(m_priceList[j], prevousPrice, m_buttonList[j]);
			}
			m_counter--;
		}
	}
	#endregion
}