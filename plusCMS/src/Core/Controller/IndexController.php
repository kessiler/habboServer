<?php
/**
 * Created by PhpStorm.
 * User: Kessiler
 * Date: 14/12/2014
 * Time: 17:41
 */
namespace Core\Controller;

use Silex\Application;
use Symfony\Component\HttpFoundation\Request;

class IndexController {

    public function indexAction(Request $request, Application $app) {
        return $app['twig']->render('index.html.twig');
    }

}