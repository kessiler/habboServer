<?php
/**
 * Created by PhpStorm.
 * User: Kessiler
 * Date: 14/12/2014
 * Time: 16:06
 */

namespace Core\Service;

use Silex\Application;
use Silex\ServiceProviderInterface;

/**
 * Class ConfigServiceProvider
 *
 * @package Service
 */
class ConfigServiceProvider implements ServiceProviderInterface {

    protected $fileLocation;

    public function __construct($fileLocation) {
        $this->fileLocation = $fileLocation;
    }

    /**
     * Registers services on the given app.
     *
     * This method should only be used to configure services and parameters.
     * It should not get services.
     *
     * @param Application $app An Application instance
     */
    public function register(Application $app) {
        $config = $this->load($this->fileLocation);
        if (isset($app['config']) && is_array($app['config'])) {
            $app['config'] = array_replace_recursive($app['config'], $config);
        } else {
            $app['config'] = $config;
        }
    }

    private function load($filename) {
        if (!file_exists($filename)) {
            throw new \InvalidArgumentException(
                sprintf("The config file '%s' does not exist.", $filename));
        }
        $json = file_get_contents($filename);
        return json_decode($json, true);
    }

    /**
     * Bootstraps the application.
     *
     * This method is called after all services are registered
     * and should be used for "dynamic" configuration (whenever
     * a service must be requested).
     *
     * @param Application $app
     */
    public function boot(Application $app) {
        // TODO: Isn't necessary implements.
    }
}