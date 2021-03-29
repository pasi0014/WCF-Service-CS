using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.IO;
using System.Web;

namespace Lab6Service
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class Service1 : IRestaurantReviewService
    {
        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }

        //Summary
        //Method returns List of restaurant names
        public List<string> GetRestaurantNames()
        { 
            List<string> names = new List<string>();
            restaurant_reviews allRestaurants = GetRestaurantsFromXml();

            if(allRestaurants != null)
            {
                foreach(restaurant_reviewsRestaurant rest in allRestaurants.restaurant)
                {
                    names.Add(rest.name);
                }
            }
            return names;
        }

        //Summary
        //Method returns a List of restaurants 
        public List<RestaurantInfo> GetAllRestaurants()
        {
            //Initialize list of restaurants
            List<RestaurantInfo> restaurants = new List<RestaurantInfo>();
            //Retrive restaurants information from xml file
            restaurant_reviews allRestaurantsFromXml = GetRestaurantsFromXml();


            if(allRestaurantsFromXml != null)
            {
                foreach(restaurant_reviewsRestaurant r in allRestaurantsFromXml.restaurant)
                {
                    //Create new Address object 
                    Address restaurantLocation = new Address();
                    restaurantLocation.City = r.address.City;
                    restaurantLocation.PostalCode = r.address.Postal_code;
                    restaurantLocation.Province = r.address.Province;
                    restaurantLocation.Street = r.address.Street;

                    //Create new RestaurantInfo object and pass Address object as a Location
                    RestaurantInfo restaurant = new RestaurantInfo
                    {
                        Id = r.id,
                        Name = r.name,
                        Location = restaurantLocation,
                        Cost = r.prices.Value,
                        Rating = r.reviews.review.rating.Value,
                        Summary = r.reviews.review.summary
                    };

                    //Add restaurant to RestaurantInfo List 
                    restaurants.Add(restaurant);
                }
            }
            //Return the list
            return restaurants;
        }

        //Summary
        //Methods retrieves specific restaurant by given id from XML file
        //
        // @param id
        //
        public RestaurantInfo GetRestaurantById(int? id)
        {
            //Check if id is not null
            if (id == null) return null;
            
            //Get all restaurants from xml file
            restaurant_reviews allRestaurantsFromXml = GetRestaurantsFromXml();
            RestaurantInfo restaurantInfo = null;
            if (allRestaurantsFromXml != null)
            {
                foreach (restaurant_reviewsRestaurant r in allRestaurantsFromXml.restaurant)
                {
                    if(r.id == id)
                    {
                        //Initialize Address object to pass it into RestaurantInfo
                        Address restaurantLocation = new Address
                        {
                            City = r.address.City,
                            PostalCode = r.address.Postal_code,
                            Province = r.address.Province,
                            Street = r.address.Street
                        };

                        //Initialize RestaurantInfo object
                        RestaurantInfo restaurant = new RestaurantInfo
                        {
                            Id = r.id,
                            Name = r.name,
                            Location = restaurantLocation,
                            Cost = r.prices.Value,
                            Rating = r.reviews.review.rating.Value,
                            Summary = r.reviews.review.summary
                        };
                        restaurantInfo = restaurant;
                    }
                }
                return restaurantInfo;
            }

            return null;
        }

        //summary
        //Method reads and deserializes xml file located on the server
        //Returns restaurant Collection
        private restaurant_reviews GetRestaurantsFromXml()
        {

            string filePath = HttpContext.Current.Server.MapPath("~/Data/RestaurantReview.xml");
            restaurant_reviews restaurants = null;
            using (FileStream file = new FileStream(filePath, FileMode.Open))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(restaurant_reviews));
                restaurants = (restaurant_reviews)serializer.Deserialize(file);
            }
            return restaurants;
        }

        //Summary
        //Method retrieves specific restaurant by given name
        //
        //@param string
        //
        public RestaurantInfo GetRestaurantByName(string name)
        {
            if (name == null || name == "") return null;

            restaurant_reviews allRestaurantsFromXml = GetRestaurantsFromXml();

            RestaurantInfo restaurant = null;

            foreach(restaurant_reviewsRestaurant r in allRestaurantsFromXml.restaurant)
            {
                if(name == r.name)
                {
                    Address restaurantLocation = new Address
                    {
                        City = r.address.City,
                        Province = r.address.Province,
                        PostalCode = r.address.Postal_code,
                        Street = r.address.Street
                    };

                    RestaurantInfo restaurantInfo = new RestaurantInfo
                    {
                        Id = r.id,
                        Name = r.name,
                        Location = restaurantLocation,
                        Cost = r.prices.Value,
                        Rating = r.reviews.review.rating.Value,
                        Summary = r.reviews.review.summary
                    };
                    
                    restaurant = restaurantInfo;
                }

            }

            return restaurant;
        }

        //Summary 
        //Methods saves any changes and updates the xml file
        //
        // @param RestaurantInfo object
        //
        // @return bool
        //
        public bool SaveRestaurant(RestaurantInfo restInfo)
        {
            if (restInfo == null) return false;

            string filePath = HttpContext.Current.Server.MapPath("~/Data/RestaurantReview.xml");

            restaurant_reviews restaurantReviews = null;

            //Read xml file
            using (FileStream file = new FileStream(filePath, FileMode.Open))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(restaurant_reviews));
                restaurantReviews = (restaurant_reviews)serializer.Deserialize(file);
            }

            restaurant_reviewsRestaurant restaurant = restaurantReviews.restaurant[restInfo.Id - 1];

            if (restInfo != null)
            {
                restaurant.id = restInfo.Id;
                restaurant.name = restInfo.Name;
                restaurant.address.Street = restInfo.Location.Street;
                restaurant.address.Province = restInfo.Location.Province;
                restaurant.address.Postal_code = restInfo.Location.PostalCode;
                restaurant.address.City = restInfo.Location.City;
                restaurant.prices.Value = (byte)restInfo.Cost;
                restaurant.reviews.review.rating.Value = (byte)restInfo.Rating;
                restaurant.reviews.review.summary = restInfo.Summary;

                using (FileStream file = new FileStream(filePath, FileMode.Create))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(restaurant_reviews));
                    serializer.Serialize(file, restaurantReviews);
                }
            }
            return true;
        }


        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            if (composite == null)
            {
                throw new ArgumentNullException("composite");
            }
            if (composite.BoolValue)
            {
                composite.StringValue += "Suffix";
            }
            return composite;
        }
    }
}
