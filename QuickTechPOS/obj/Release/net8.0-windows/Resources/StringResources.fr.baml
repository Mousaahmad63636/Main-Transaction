   M S B A M L   `   `   `  ���� O  Kmscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e0891&clr-namespace:System;assembly=mscorlibSystem  W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089   ApplicationTitle  Welcome  SuccessTitle  
ErrorTitle  WarningTitle  	InfoTitle  RecoveryCenter 
 CashIn  CashOut 	 CashInTitle 
 	CashInBtn  CashInAmount  ExecuteCashIn  PrintJobsStatus  PrintQueueManager  Created 
 Status  Attempts  LastAttempt 	 Retry 
 Remove  Refresh  
SaveButton  CancelButton  CloseButton  	AddButton  
EditButton  DeleteButton  SearchButton  PrintButton  SelectButton  
ScanButton   BoxPrice ! ItemsPerBox " EditingTransaction # EditingMessage $ StartNewTransaction % PrintReceiptTooltip & ViewingTransaction ' EditingTransactionNumber ( DrawerRequiredMessage ) TransactionComplete * ReceiptPrinted + NoTransactionFound , DrawerStatus - 	AmountDue . 	ChangeDue / TypeProductName 0 FileMenu 1 EditMenu 2 ViewMenu 3 	ToolsMenu 4 LanguageMenu 5 HelpMenu 6 
LogoutMenu 7 SettingsMenu 8 
LoginTitle 9 Username : Password ; SignInButton < IncorrectCredentials = ScanBarcode > SearchProducts ? ShoppingCart @ Customer A SelectCustomer B AddNewCustomer 	C Total D 	AmountLBP E Payment F CompleteSale G 	ClearCart H Product 	I Price J Quantity K Discount L 	AddToDebt M AmountToDebt N TransactionLookup O TransactionNumber P LookupButton Q EditTransaction R SaveChanges S CancelChanges T NewSale U PrintReceipt V EnterScanBarcode 
W Drawer X 
DrawerOpen Y DrawerClosed Z OpenDrawerBtn [ CloseDrawerBtn \ 
CashOutBtn ] OpeningBalance ^ ClosingBalance _ OpenDrawerTitle ` CloseDrawerTitle a CashOutTitle 	b Notes c OptionalNotes d RequiredNotes e OpenDrawerButton f DrawerReport g Cashier h OpenedAt i SystemBalance j CurrentBalance k 
DailySales l CashOutAmount m 
TotalSales n NetSales o CountCashMessage p DifferenceText q PrintAfterClosing r ExecuteCashOut s DrawerId t ErrorDrawerNull u DialogInitError v DialogErrorTitle w CustomerNameRequired x CustomerPhoneRequired y ErrorAddingCustomer z ErrorLoadingCustomers { ErrorSearchingCustomers | PleaseSelectCustomer } 	PriceMode ~ 
RetailMode  WholesaleMode � LogoutButton � LanguageSettings � SelectLanguage � LanguageChangeNote � LanguageChangeSuccess � CustomerTitle � CustomerName � CustomerPhone � CustomerEmail � CustomerAddress � CustomerBalance � AddCustomerTitle � SelectCustomerTitle � HoldCart � RestoreCart � RestoreCartTitle � RestoreButton 
� CartId � 	ItemCount � 	CreatedAt � PleaseSelectCart � ValidationError � LoadingProducts � ReadyStatus � NoProductsAvailable � EmptyCartWarning � NegativePaymentError � WalkInCustomerDebtError�� F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     :x,http://schemas.microsoft.com/winfx/2006/xaml     1system&clr-namespace:System;assembly=mscorlib  %�  &	        &	      &	 *     &	 ;     &	 K     &	 b     &	 w     &	 �     &	 �     &		 �     &	
 �     &	 �     &	     &	 (    &	 P    &	 {    &	 �    &	 �    &	 �    &	 �    &	 �    &	 �    &	     &	     &	 -    &	 =    &	 N    &	 `    &	 s    &	 �    &	 �    &	 �    &	  �    &	! �    &	" �    &	#     &	$ U    &	% �    &	& �    &	' �    &	( �    &	) H    &	* w    &	+ �    &	, �    &	- �    &	. �    &	/ �    &	0      &	1 1    &	2 C    &	3 V    &	4 f    &	5 v    &	6 �    &	7 �    &	8 �    &	9 �    &	: �    &	; �    &	< 	    &	= S    &	> p    &	? �    &	@ �    &	A �    &	B �    &	C �    &	D     &	E     &	F 0    &	G I    &	H b    &	I s    &	J �    &	K �    &	L �    &	M �    &	N �    &	O     &	P 5    &	Q I    &	R g    &	S �    &	T �    &	U �    &	V �    &	W 	    &	X 	    &	Y &	    &	Z 6	    &	[ M	    &	\ d	    &	] u	    &	^ �	    &	_ �	    &	` �	    &	a �	    &	b 
    &	c 
    &	d .
    &	e K
    &	f b
    &	g |
    &	h �
    &	i �
    &	j �
    &	k �
    &	l �
    &	m 	    &	n !    &	o 8    &	p �    &	q �    &	r �    &	s �    &	t 
    &	u Y    &	v �    &	w �    &	x �    &	y     &	z G    &	{ w    &	| �    &	} �    &	~ �    &	     &	�     &	� *    &	� I    &	� j    &	� �    &	� �    &	� �    &	� �    &	�     &	�     &	� .    &	� =    &	� ]    &	� {    &	� �    &	� �    &	� �    &	�      &	�     &	�     &	� 1    &	� i    &	� �    &	� �    &	� �    &	� �    &	�     &	� P    ��QuickTech PDV����	Bienvenue����Succès����
Erreur����Avertissement����Information����Centre de Récupération����Encaissement����Décaissement����Encaissement����Encaissement����Montant à Ajouter����Ajouter Espèces����"État des Tâches d'Impression����%!Gestionnaire de File d'Impression����
Créé����
Statut����
Tentatives����Dernière Tentative����
Réessayer����	Supprimer����
Actualiser����Enregistrer����Annuler����
Fermer����Ajouter����Modifier����	Supprimer����
Rechercher����Imprimer����Sélectionner����Scanner����Prix boîte:����articles/boîte)����MODIFICATION TRANSACTION����<8(faites des modifications et enregistrez quand terminé)����&"Démarrer une nouvelle transaction����,(Imprimer un reçu pour cette transaction����Consultation Transaction #����MODIFICATION Transaction #����GCVous devez ouvrir un tiroir pour continuer à utiliser le système.����)%Transaction complétée avec succès.����Reçu imprimé.����Transaction non trouvée.����État du Tiroir����Montant Dû����Monnaie à Rendre����Tapez le nom du produit...����Fichier����Édition����	Affichage����
Outils����
Langue����Aide����Déconnexion����Paramètres����	Connexion����Nom d'utilisateur����Mot de passe����Se connecter����D@Nom d'utilisateur ou mot de passe invalide. Veuillez réessayer.����SCANNER CODE-BARRES����RECHERCHER PRODUITS����
PANIER����
CLIENT����Sélectionner Client����Ajouter Nouveau����MONTANT TOTAL (USD)����MONTANT EN LBP����MONTANT PAYÉ����FINALISER VENTE����Vider le panier����PRODUIT����PRIX����QTÉ����
REMISE����2.Ajouter le solde restant à la dette du client����"MONTANT À AJOUTER À LA DETTE����RECHERCHE TRANSACTION����Transaction #����
Rechercher����MODIFIER TRANSACTION����ENREGISTRER MODIFICATIONS����ANNULER MODIFICATIONS����NOUVELLE VENTE����IMPRIMER REÇU����'#Entrez ou scannez le code-barres...����
TIROIR����
OUVERT����
FERMÉ����Ouvrir Tiroir����Fermer Tiroir����Retrait����Solde d'ouverture����Solde de fermeture����Ouvrir Tiroir-Caisse����Fermer Tiroir-Caisse����Retrait d'espèces����	Notes����Notes (optionnel)����Notes (obligatoire)����Ouvrir Tiroir����Imprimer Rapport����Caissier����	Ouvert à����Solde Système����Solde Actuel����Ventes Journalières����Montant du Retrait����Ventes Totales����Ventes Nettes����MIVeuillez compter l'argent dans le tiroir et saisir le solde de fermeture:����%!Différence (Compté - Système):����Imprimer après fermeture����Exécuter le retrait����	ID Tiroir����IEImpossible de fermer le tiroir: Aucune information de tiroir fournie.����LHErreur d'initialisation de la boîte de dialogue de fermeture du tiroir:����Erreur de dialogue���� Le nom du client est requis.����)%Le numéro de téléphone est requis.����%!Erreur lors de l'ajout du client:����*&Erreur lors du chargement des clients:����,(Erreur lors de la recherche des clients:����3/Veuillez sélectionner un client dans la liste.����	Mode Prix����Détail����Gros����Déconnexion����Paramètres de Langue����Sélectionner la Langue����D@L'application appliquera le changement de langue immédiatement.����"Langue changée avec succès !����
Client����Nom����Téléphone����	Email����Adresse����	Solde����Ajouter Nouveau Client����Sélectionner Client����Mettre en Attente����Restaurer Panier����Restaurer Panier en Attente����%!Restaurer le Panier Sélectionné����ID����Articles����	Créé le����2.Veuillez sélectionner un panier à restaurer.����Erreur de Validation����Chargement des produits...����
Prêt.����Aucun produit disponible.����0,Le panier est vide. Impossible de finaliser.����62Le montant du paiement ne peut pas être négatif.����ieImpossible d'ajouter une dette à un client de passage. Veuillez sélectionner un client enregistré.��